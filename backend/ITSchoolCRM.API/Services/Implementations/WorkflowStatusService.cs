using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.WorkflowStatuses;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис статусов workflow (этапов).
/// </summary>
/// <remarks>
/// КЭШИРОВАНИЕ: сервис — точка записи, а не чтения. Горячее чтение
/// схемы (статусы + переходы) обслуживает WorkflowService.GetByIdAsync
/// под версионированным ключом workflow:{id}:v{workflow:{id}:version}.
/// Этот сервис использует ТОТ ЖЕ версионный ключ для инвалидации —
/// после любой мутации здесь кэш схемы сбрасывается, и фронт при
/// следующем открытии карточки получает новую карту этапов. Отдельный
/// кэш списков статусов (GetByWorkflowIdAsync, GetByIdAsync) сознательно
/// не вводится: это дублирование данных схемы во втором кэшированном
/// источнике, а значит — потенциальный рассинхрон.
///
/// ИНВАЛИДАЦИЯ: Create/Update/Delete после SaveChangesAsync сбрасывают
/// два счётчика:
///   • workflow:{workflowId}:version — кэш схемы этого workflow
///     (общий ключ с WorkflowService/WorkflowTransitionService);
///   • interaction:version — карточки и списки взаимодействий хранят
///     CurrentStatusName, а история — FromStatusName/ToStatusName.
///     Переименование этапа админом (единственная роль с этим правом)
///     обязано отразиться везде мгновенно.
/// </remarks>
public class WorkflowStatusService : IWorkflowStatusService
{
    private readonly CrmDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICacheService _cache;

    public WorkflowStatusService(
        CrmDbContext context,
        IAuditService auditService,
        ICacheService cache)
    {
        _context = context;
        _auditService = auditService;
        _cache = cache;
    }

    /// <summary>
    /// Сброс кэша схемы конкретного workflow и всех взаимодействий.
    /// workflowId известен во всех трёх методах записи (в Create —
    /// из DTO, в Update/Delete — из загруженной сущности статуса).
    /// </summary>
    private async Task InvalidateWorkflowAsync(
        int workflowId,
        CancellationToken cancellationToken)
    {
        await _cache.BumpVersionAsync(CacheKeys.WorkflowVersion(workflowId), cancellationToken);
        await _cache.BumpVersionAsync(CacheKeys.InteractionVersionKey, cancellationToken);
    }

    // Единая проекция сущности -> DTO
    private static IQueryable<WorkflowStatusDto> ProjectToDto(IQueryable<workflow_status> query)
    {
        return query.Select(x => new WorkflowStatusDto
        {
            Id = x.workflow_statuses_id,
            WorkflowId = x.workflow_id,
            Name = x.name,
            Description = x.description,
            SortOrder = x.sort_order,
            IsInitial = x.is_initial,
            IsFinal = x.is_final
        });
    }

    public async Task<List<WorkflowStatusDto>> GetByWorkflowIdAsync(
        int workflowId,
        CancellationToken cancellationToken)
    {
        // БЕЗ СОБСТВЕННОГО КЭША: фронт берёт схему целиком через
        // GET /Workflows/{id} (кэшируется в WorkflowService),
        // этот эндпоинт — для точечных сценариев. Повторное
        // кэширование того же набора под другим ключом создало
        // бы два источника истины об этапах — не делаем.
        return await ProjectToDto(
                _context.workflow_statuses
                    .AsNoTracking()
                    .Where(x => x.workflow_id == workflowId)
                    .OrderBy(x => x.sort_order))
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowStatusDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        // БЕЗ КЭША: точечный вызов (подсказки, валидации), обращений мало.
        return await ProjectToDto(
                _context.workflow_statuses
                    .AsNoTracking()
                    .Where(x => x.workflow_statuses_id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<WorkflowStatusDto?> CreateAsync(
        CreateWorkflowStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!dto.WorkflowId.HasValue)
        {
            throw new ArgumentException("WorkflowId обязателен.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Название статуса обязательно.");
        }

        var workflow = await _context.workflows
            .FirstOrDefaultAsync(
                x => x.workflows_id == dto.WorkflowId.Value,
                cancellationToken);

        if (workflow is null)
        {
            return null;
        }

        // Начальный статус в workflow один: повторное назначение запрещено
        if (dto.IsInitial == true)
        {
            var existingInitial = await _context.workflow_statuses
                .AnyAsync(
                    x =>
                        x.workflow_id == dto.WorkflowId.Value &&
                        x.is_initial == true,
                    cancellationToken);

            if (existingInitial)
            {
                throw new InvalidOperationException(
                    "В workflow уже существует начальный статус.");
            }
        }

        var status = new workflow_status
        {
            workflow_id = dto.WorkflowId,
            name = dto.Name.Trim(),
            description = dto.Description,
            sort_order = dto.SortOrder ?? 0,
            is_initial = dto.IsInitial ?? false,
            is_final = dto.IsFinal ?? false
        };

        _context.workflow_statuses.Add(status);

        workflow.version = (workflow.version ?? 0) + 1;
        workflow.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            "CREATE",
            "workflow_status",
            status.workflow_statuses_id,
            null,
            new
            {
                status.workflow_statuses_id,
                status.workflow_id,
                status.name,
                status.description,
                status.sort_order,
                status.is_initial,
                status.is_final
            },
            cancellationToken);

        // Главный сценарий сервиса — «Добавить статус» админом.
        // Новый этап обязан появиться на карте workflow у ВСЕХ
        // взаимодействий этого workflow немедленно (фронт перечитывает
        // схему после закрытия модалки). workflowId из DTO.
        await InvalidateWorkflowAsync(dto.WorkflowId.Value, cancellationToken);

        return new WorkflowStatusDto
        {
            Id = status.workflow_statuses_id,
            WorkflowId = status.workflow_id,
            Name = status.name,
            Description = status.description,
            SortOrder = status.sort_order,
            IsInitial = status.is_initial,
            IsFinal = status.is_final
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateWorkflowStatusDto dto,
        CancellationToken cancellationToken)
    {
        var status = await _context.workflow_statuses
            .FirstOrDefaultAsync(x => x.workflow_statuses_id == id, cancellationToken);

        if (status is null)
        {
            return false;
        }

        // Начальный статус в workflow один (с исключением самого себя)
        if (dto.IsInitial == true)
        {
            var anotherInitial = await _context.workflow_statuses
                .AnyAsync(
                    x =>
                        x.workflow_id == status.workflow_id &&
                        x.workflow_statuses_id != id &&
                        x.is_initial == true,
                    cancellationToken);

            if (anotherInitial)
            {
                throw new InvalidOperationException(
                    "В workflow уже существует начальный статус.");
            }
        }

        var oldData = new
        {
            status.workflow_statuses_id,
            status.workflow_id,
            status.name,
            status.description,
            status.sort_order,
            status.is_initial,
            status.is_final
        };

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            status.name = dto.Name.Trim();
        }

        if (dto.Description is not null)
        {
            status.description = dto.Description;
        }

        if (dto.SortOrder.HasValue)
        {
            status.sort_order = dto.SortOrder.Value;
        }

        if (dto.IsInitial.HasValue)
        {
            status.is_initial = dto.IsInitial.Value;
        }

        if (dto.IsFinal.HasValue)
        {
            status.is_final = dto.IsFinal.Value;
        }

        var workflow = await _context.workflows
            .FirstOrDefaultAsync(
                x => x.workflows_id == status.workflow_id,
                cancellationToken);

        if (workflow is not null)
        {
            workflow.version = (workflow.version ?? 0) + 1;
            workflow.updated_at = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            "UPDATE",
            "workflow_status",
            status.workflow_statuses_id,
            oldData,
            new
            {
                status.workflow_statuses_id,
                status.workflow_id,
                status.name,
                status.description,
                status.sort_order,
                status.is_initial,
                status.is_final
            },
            cancellationToken);

        // Переименование (право только у администратора), смена описания,
        // порядка (sort_order — порядок узлов на карте) или флагов
        // is_initial/is_final — всё это зашито в кэш схемы и в кэши
        // карточек (CurrentStatusName). workflow_id — из сущности.
        await InvalidateWorkflowAsync(status.workflow_id!.Value, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var status = await _context.workflow_statuses
            .FirstOrDefaultAsync(x => x.workflow_statuses_id == id, cancellationToken);

        if (status is null)
        {
            return false;
        }

        // Удаление возможно, только если статус нигде не используется:
        // ни как текущий у взаимодействий, ни в истории, ни в переходах.
        // Иначе остались бы ссылки на несуществующий этап.
        var used = await _context.interactions
            .AnyAsync(x => x.current_status_id == id, cancellationToken);

        if (used)
        {
            throw new InvalidOperationException(
                "Нельзя удалить статус, который используется interaction.");
        }

        var hasHistory = await _context.interaction_status_histories
            .AnyAsync(
                x =>
                    x.from_status_id == id ||
                    x.to_status_id == id,
                cancellationToken);

        if (hasHistory)
        {
            throw new InvalidOperationException(
                "Нельзя удалить статус, который присутствует в истории.");
        }

        var hasTransitions = await _context.workflow_transitions
            .AnyAsync(
                x =>
                    x.from_status_id == id ||
                    x.to_status_id == id,
                cancellationToken);

        if (hasTransitions)
        {
            throw new InvalidOperationException(
                "Сначала удалите переходы, связанные со статусом.");
        }

        var oldData = new
        {
            status.workflow_statuses_id,
            status.workflow_id,
            status.name,
            status.description,
            status.sort_order,
            status.is_initial,
            status.is_final
        };

        var workflow = await _context.workflows
            .FirstOrDefaultAsync(
                x => x.workflows_id == status.workflow_id,
                cancellationToken);

        _context.workflow_statuses.Remove(status);

        if (workflow is not null)
        {
            workflow.version = (workflow.version ?? 0) + 1;
            workflow.updated_at = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            "DELETE",
            "workflow_status",
            id,
            oldData,
            null,
            cancellationToken);

        // Этап удалён — карта workflow и допустимые переходы изменились.
        await InvalidateWorkflowAsync(status.workflow_id!.Value, cancellationToken);

        return true;
    }
}