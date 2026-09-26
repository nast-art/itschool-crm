using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.WorkflowStatuses;
using ITSchoolCRM.API.DTOs.WorkflowTransitions;
using ITSchoolCRM.API.DTOs.Workflows;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис workflow.
/// </summary>
/// <remarks>
/// КЭШИРОВАНИЕ (KeyDB, cache-aside):
///   • GetByIdAsync → версионированный ключ workflow:{id}:
///     {workflow:{id}:version}. Схема (статусы + переходы) грузится
///     при открытии КАЖДОЙ карточки взаимодействия на фронте — это
///     горячий запрос. Скоп пользователя не нужен: схема единая для
///     всех ролей, а разграничение доступа относится к взаимодействиям,
///     а не к workflow;
///   • GetAllAsync / GetActiveAsync → без кэша: вызываются только из
///     админки и списков выбора workflow, обращений мало, а схемы в них
///     тяжёлые (все статусы + переходы всех workflow) — кэш разумнее
///     вкладывать в GetByIdAsync;
///   • CreateAsync → без инвалидации: новый workflow ещё ни у кого
///     не закэширован.
///
/// ИНВАЛИДАЦИЯ: UpdateAsync/DeleteAsync сбрасывают ДВА счётчика:
///   • workflow:{id}:version — саму схему этого workflow (тот же
///     версионный ключ используют WorkflowStatusService и
///     WorkflowTransitionService — все трое сбрасывают одну версию);
///   • interaction:version — карточки взаимодействий хранят
///     WorkflowName и текущие статусы: деактивация workflow или смена
///     его имени должны отразиться в списках.
/// Мутации статусов и переходов сбрасывают те же ключи — поэтому
/// «Добавить статус» админом мгновенно обновляет карту этапов у всех
/// взаимодействий этого workflow.
/// </remarks>
public class WorkflowService : IWorkflowService
{
    private readonly CrmDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    public WorkflowService(
        CrmDbContext context,
        IAuditService auditService,
        ICacheService cache,
        CacheOptions cacheOptions)
    {
        _context = context;
        _auditService = auditService;
        _cache = cache;
        _cacheOptions = cacheOptions;
    }

    /// <summary>
    /// Сброс кэша конкретного workflow и всех взаимодействий.
    /// Вызывается после любой мутации workflow (имя, активность).
    /// Мутации статусов/переходов сбрасывают те же счётчики из своих
    /// сервисов — ключ versionId общий для всех трёх.
    /// </summary>
    private async Task InvalidateWorkflowAsync(
        int workflowId,
        CancellationToken cancellationToken)
    {
        await _cache.BumpVersionAsync(CacheKeys.WorkflowVersion(workflowId), cancellationToken);
        await _cache.BumpVersionAsync(CacheKeys.InteractionVersionKey, cancellationToken);
    }

    // Полная проекция workflow -> DTO со схемой (статусы + переходы).
    private static IQueryable<WorkflowDto> ProjectFullDto(IQueryable<workflow> query)
    {
        return query.Select(x => new WorkflowDto
        {
            Id = x.workflows_id,
            Name = x.name,
            Description = x.description,
            IsActive = x.is_active,
            Version = x.version,
            Statuses = x.workflow_statuses
                .OrderBy(s => s.sort_order)
                .Select(s => new WorkflowStatusDto
                {
                    Id = s.workflow_statuses_id,
                    WorkflowId = s.workflow_id,
                    Name = s.name,
                    Description = s.description,
                    SortOrder = s.sort_order,
                    IsInitial = s.is_initial,
                    IsFinal = s.is_final
                })
                .ToList(),
            Transitions = x.workflow_transitions
                .Select(t => new WorkflowTransitionDto
                {
                    Id = t.workflow_transitions_id,
                    WorkflowId = t.workflow_id,
                    FromStatusId = t.from_status_id,
                    FromStatusName = t.from_status != null ? t.from_status.name : null,
                    ToStatusId = t.to_status_id,
                    ToStatusName = t.to_status != null ? t.to_status.name : null
                })
                .ToList()
        });
    }

    // Краткая проекция без схемы (для списков выбора).
    private static IQueryable<WorkflowDto> ProjectSummaryDto(IQueryable<workflow> query)
    {
        return query.Select(x => new WorkflowDto
        {
            Id = x.workflows_id,
            Name = x.name,
            Description = x.description,
            IsActive = x.is_active,
            Version = x.version
        });
    }

    public async Task<List<WorkflowDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        // БЕЗ КЭША ОСОЗНАННО: полные схемы ВСЕХ workflow — тяжёлый результат,
        // запрашивается только из админки. Кэшировать мегабайты схем ради
        // пары вызовов в час — пустая трата памяти KeyDB.
        return await ProjectFullDto(
                _context.workflows
                    .AsNoTracking()
                    .OrderBy(x => x.name))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        // Схема workflow одинакова для всех пользователей — ключ без скопа.
        // Добавление статуса (WorkflowStatusService.CreateAsync) делает INCR
        // workflow:{id}:version — карта этапов обновляется у всех карточек
        // этого workflow, не трогая кэш других workflow.
        return await _cache.GetOrCreateAsync(
            CacheKeys.Workflow(id),
            _cacheOptions.InteractionTtl,
            _ => LoadByIdFromDatabaseAsync(id, cancellationToken),
            cancellationToken);
    }

    /// <summary>Загрузка схемы workflow из БД одним запросом (промах кэша).</summary>
    private async Task<WorkflowDto?> LoadByIdFromDatabaseAsync(int id, CancellationToken cancellationToken)
    {
        return await ProjectFullDto(
                _context.workflows
                    .AsNoTracking()
                    .Where(x => x.workflows_id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<WorkflowDto>> GetActiveAsync(CancellationToken cancellationToken)
    {
        // БЕЗ КЭША, как GetAllAsync: лёгкий список без схем,
        // запрашивается из форм создания взаимодействия.
        return await ProjectSummaryDto(
                _context.workflows
                    .AsNoTracking()
                    .Where(x => x.is_active == true)
                    .OrderBy(x => x.name))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowDto> CreateAsync(CreateWorkflowDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Название workflow обязательно.");
        }

        var exists = await _context.workflows
            .AnyAsync(x => x.name == dto.Name, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Workflow с таким названием уже существует.");
        }

        var workflow = new workflow
        {
            name = dto.Name.Trim(),
            description = dto.Description,
            is_active = true,
            version = 1,
            created_at = DateTime.UtcNow
        };

        _context.workflows.Add(workflow);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            "CREATE",
            "workflow",
            workflow.workflows_id,
            null,
            new
            {
                workflow.workflows_id,
                workflow.name,
                workflow.description,
                workflow.is_active,
                workflow.version
            },
            cancellationToken);

        // Инвалидация не нужна: workflow только что создан, его схема
        // ещё ни у кого не закэширована. DTO собираем руками — он свежий
        // по определению.
        return new WorkflowDto
        {
            Id = workflow.workflows_id,
            Name = workflow.name,
            Description = workflow.description,
            IsActive = workflow.is_active,
            Version = workflow.version
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateWorkflowDto dto, CancellationToken cancellationToken)
    {
        var workflow = await _context.workflows
            .FirstOrDefaultAsync(x => x.workflows_id == id, cancellationToken);

        if (workflow is null)
        {
            return false;
        }

        var oldData = new
        {
            workflow.workflows_id,
            workflow.name,
            workflow.description,
            workflow.is_active,
            workflow.version
        };

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            workflow.name = dto.Name.Trim();
        }

        if (dto.Description is not null)
        {
            workflow.description = dto.Description;
        }

        if (dto.IsActive.HasValue)
        {
            workflow.is_active = dto.IsActive.Value;
        }

        workflow.version = (workflow.version ?? 0) + 1;
        workflow.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            "UPDATE",
            "workflow",
            workflow.workflows_id,
            oldData,
            new
            {
                workflow.workflows_id,
                workflow.name,
                workflow.description,
                workflow.is_active,
                workflow.version
            },
            cancellationToken);

        // Имя workflow видно в карточках и списках взаимодействий
        // (WorkflowName), активность влияет на выбор при создании.
        // Схема могла и не измениться по составу, но сброс кэша дешевле,
        // чем анализ «что именно поменялось».
        await InvalidateWorkflowAsync(id, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var workflow = await _context.workflows
            .FirstOrDefaultAsync(x => x.workflows_id == id, cancellationToken);

        if (workflow is null)
        {
            return false;
        }

        var oldData = new
        {
            workflow.workflows_id,
            workflow.name,
            workflow.description,
            workflow.is_active,
            workflow.version
        };

        // Мягкое удаление: существующие взаимодействия ссылаются
        // на workflow, физическое удаление сломало бы историю.
        workflow.is_active = false;
        workflow.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            "DELETE",
            "workflow",
            workflow.workflows_id,
            oldData,
            new
            {
                workflow.workflows_id,
                workflow.is_active
            },
            cancellationToken);

        // Деактивация должна исчезнуть из карточек взаимодействий
        // (WorkflowName + is_active). GetActiveAsync без кэша и так
        // перечитает БД.
        await InvalidateWorkflowAsync(id, cancellationToken);

        return true;
    }
}