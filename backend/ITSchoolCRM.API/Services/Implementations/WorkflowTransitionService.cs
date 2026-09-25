using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.WorkflowTransitions;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис переходов workflow (рёбра графа этапов).
///
/// КЭШИРОВАНИЕ: сервис — точка записи. Горячее чтение схемы
/// (статусы + переходы вместе) обслуживает WorkflowService.
/// GetByIdAsync под ключом workflow:{id}:v{workflow:{id}:version}.
/// Этот сервис сбрасывает ТОТ ЖЕ версионный ключ — после любой
/// мутации здесь кэш схемы инвалидируется, и список разрешённых
/// переходов на фронте обновляется при следующем запросе.
/// Собственный кэш списков переходов не вводится: переходы
/// бессмысленны без статусов, кэшировать их отдельно — создавать
/// второй источник истины о схеме.
///
/// ИНВАЛИДАЦИЯ: Create/Update/Delete после SaveChangesAsync
/// сбрасывают два счётчика:
///   • workflow:{workflowId}:version — кэш схемы (общий ключ
///     с WorkflowService/WorkflowStatusService);
///   • interaction:version — набор допустимых переходов зашит
///     в блок «Перевод статуса» карточки (фронт строит его из
///     transitions схемы): новый или удалённый переход обязан
///     отразиться там немедленно, иначе менеджер увидит
///     устаревший список «Новый статус».
///
/// IsAllowedAsync — СОЗНАТЕЛЬНО БЕЗ КЭША: метод вызывается
/// внутри InteractionService.ChangeStatusAsync ВО ВРЕМЯ
/// операции записи. Кэшированная проверка допустимости перехода
/// дала бы рассинхрон между проверкой и реальной схемой
/// (админ удалил переход, а кэш ещё разрешает). Это
/// безопасность, а не производительность: один индексный
/// запрос к workflow_transitions занимает доли миллисекунды.
/// </summary>
public class WorkflowTransitionService : IWorkflowTransitionService
{
    private readonly CrmDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICacheService _cache;

    public WorkflowTransitionService(
        CrmDbContext context,
        IAuditService auditService,
        ICacheService cache)
    {
        _context = context;
        _auditService = auditService;
        _cache = cache;
    }

    /// <summary>
    /// Сброс кэша схемы конкретного workflow и всех
    /// взаимодействий. workflowId известен во всех трёх методах
    /// записи (в Create — из DTO, в Update/Delete — из загруженной
    /// сущности перехода). Ключ версии общий с WorkflowService и
    /// WorkflowStatusService — тройное согласование инвалидации
    /// не требуется, все трое бампят один и тот же счётчик.
    /// </summary>
    private async Task InvalidateWorkflowAsync(
        int workflowId,
        CancellationToken cancellationToken)
    {
        await _cache.BumpVersionAsync(
            CacheKeys.WorkflowVersion(workflowId),
            cancellationToken);

        await _cache.BumpVersionAsync(
            CacheKeys.InteractionVersionKey,
            cancellationToken);
    }

    public async Task<List<WorkflowTransitionDto>> GetByWorkflowIdAsync(
        int workflowId,
        CancellationToken cancellationToken)
    {
        // БЕЗ СОБСТВЕННОГО КЭША: переходы запрашиваются фронтом
        // только в составе полной схемы GET /Workflows/{id}
        // (кэшируется в WorkflowService). Отдельный кэш здесь
        // дублировал бы данные схемы и создавал бы риск
        // рассинхрона «список переходов обновился, а схема —
        // нет». Читаем напрямую из БД — запрос быстрый
        // (индекс по workflow_id).
        return await _context.workflow_transitions
            .AsNoTracking()
            .Where(x => x.workflow_id == workflowId)
            .OrderBy(x => x.from_status_id)
            .ThenBy(x => x.to_status_id)
            .Select(x => new WorkflowTransitionDto
            {
                Id = x.workflow_transitions_id,

                WorkflowId = x.workflow_id,

                FromStatusId = x.from_status_id,

                FromStatusName =
                    x.from_status != null
                        ? x.from_status.name
                        : null,

                ToStatusId = x.to_status_id,

                ToStatusName =
                    x.to_status != null
                        ? x.to_status.name
                        : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowTransitionDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        // БЕЗ КЭША: точечный вызов (админка, отладка),
        // обращений мало.
        return await _context.workflow_transitions
            .AsNoTracking()
            .Where(x =>
                x.workflow_transitions_id == id)
            .Select(x => new WorkflowTransitionDto
            {
                Id = x.workflow_transitions_id,

                WorkflowId = x.workflow_id,

                FromStatusId = x.from_status_id,

                FromStatusName =
                    x.from_status != null
                        ? x.from_status.name
                        : null,

                ToStatusId = x.to_status_id,

                ToStatusName =
                    x.to_status != null
                        ? x.to_status.name
                        : null
            })
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public async Task<WorkflowTransitionDto?> CreateAsync(
        CreateWorkflowTransitionDto dto,
        CancellationToken cancellationToken)
    {
        if (!dto.WorkflowId.HasValue ||
            !dto.FromStatusId.HasValue ||
            !dto.ToStatusId.HasValue)
        {
            throw new ArgumentException(
                "WorkflowId, FromStatusId и ToStatusId обязательны.");
        }

        if (dto.FromStatusId ==
            dto.ToStatusId)
        {
            throw new InvalidOperationException(
                "Нельзя создать переход статуса в самого себя.");
        }

        var workflow =
            await _context.workflows
                .FirstOrDefaultAsync(
                    x =>
                        x.workflows_id ==
                        dto.WorkflowId.Value,
                    cancellationToken);

        if (workflow is null)
        {
            return null;
        }

        if (workflow.is_active != true)
        {
            throw new InvalidOperationException(
                "Нельзя создать переход в неактивном workflow.");
        }

        var fromStatus =
            await _context.workflow_statuses
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_statuses_id ==
                            dto.FromStatusId.Value &&
                        x.workflow_id ==
                            dto.WorkflowId.Value,
                    cancellationToken);

        if (fromStatus is null)
        {
            throw new InvalidOperationException(
                "Исходный статус не принадлежит указанному workflow.");
        }

        var toStatus =
            await _context.workflow_statuses
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_statuses_id ==
                            dto.ToStatusId.Value &&
                        x.workflow_id ==
                            dto.WorkflowId.Value,
                    cancellationToken);

        if (toStatus is null)
        {
            throw new InvalidOperationException(
                "Конечный статус не принадлежит указанному workflow.");
        }

        var exists =
            await _context.workflow_transitions
                .AnyAsync(
                    x =>
                        x.workflow_id ==
                            dto.WorkflowId.Value &&
                        x.from_status_id ==
                            dto.FromStatusId.Value &&
                        x.to_status_id ==
                            dto.ToStatusId.Value,
                    cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "Такой переход уже существует.");
        }

        var transition = new workflow_transition
        {
            workflow_id = dto.WorkflowId,
            from_status_id = dto.FromStatusId,
            to_status_id = dto.ToStatusId
        };

        _context.workflow_transitions.Add(
            transition);

        workflow.version =
            (workflow.version ?? 0) + 1;

        workflow.updated_at =
            DateTime.UtcNow;

        await _context.SaveChangesAsync(
            cancellationToken);

        await _auditService.WriteAsync(
            "CREATE",
            "workflow_transition",
            transition.workflow_transitions_id,
            null,
            new
            {
                transition.workflow_transitions_id,
                transition.workflow_id,
                transition.from_status_id,
                transition.to_status_id
            },
            cancellationToken);

        // НОВОЕ (кэш): добавлено разрешение на переход
        // (например, возврат на шаг назад там, где его не было —
        // уточнение заказчика 16.09.2026 12:15: «Возврат на
        // предыдущий шаг возможен»). Список «Новый статус» в
        // блоке перевода карточки строится из transitions схемы —
        // без инвалидации менеджер не увидел бы новый переход,
        // пока не истёк бы TTL. workflowId — из DTO (проверен
        // выше, workflow найден).
        await InvalidateWorkflowAsync(
            dto.WorkflowId.Value,
            cancellationToken);

        return new WorkflowTransitionDto
        {
            Id = transition.workflow_transitions_id,

            WorkflowId = transition.workflow_id,

            FromStatusId =
                transition.from_status_id,

            FromStatusName =
                fromStatus.name,

            ToStatusId =
                transition.to_status_id,

            ToStatusName =
                toStatus.name
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateWorkflowTransitionDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.FromStatusId ==
            dto.ToStatusId)
        {
            throw new InvalidOperationException(
                "Нельзя создать переход статуса в самого себя.");
        }

        var transition =
            await _context.workflow_transitions
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_transitions_id ==
                        id,
                    cancellationToken);

        if (transition is null)
        {
            return false;
        }

        var workflow =
            await _context.workflows
                .FirstOrDefaultAsync(
                    x =>
                        x.workflows_id ==
                        transition.workflow_id,
                    cancellationToken);

        if (workflow is null)
        {
            return false;
        }

        if (workflow.is_active != true)
        {
            throw new InvalidOperationException(
                "Нельзя изменять переход неактивного workflow.");
        }

        var fromStatus =
            await _context.workflow_statuses
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_statuses_id ==
                            dto.FromStatusId &&
                        x.workflow_id ==
                            transition.workflow_id,
                    cancellationToken);

        if (fromStatus is null)
        {
            throw new InvalidOperationException(
                "Исходный статус не принадлежит workflow.");
        }

        var toStatus =
            await _context.workflow_statuses
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_statuses_id ==
                            dto.ToStatusId &&
                        x.workflow_id ==
                            transition.workflow_id,
                    cancellationToken);

        if (toStatus is null)
        {
            throw new InvalidOperationException(
                "Конечный статус не принадлежит workflow.");
        }

        var exists =
            await _context.workflow_transitions
                .AnyAsync(
                    x =>
                        x.workflow_transitions_id != id &&
                        x.workflow_id ==
                            transition.workflow_id &&
                        x.from_status_id ==
                            dto.FromStatusId &&
                        x.to_status_id ==
                            dto.ToStatusId,
                    cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "Такой переход уже существует.");
        }

        var oldData = new
        {
            transition.workflow_transitions_id,
            transition.workflow_id,
            transition.from_status_id,
            transition.to_status_id
        };

        transition.from_status_id =
            dto.FromStatusId;

        transition.to_status_id =
            dto.ToStatusId;

        workflow.version =
            (workflow.version ?? 0) + 1;

        workflow.updated_at =
            DateTime.UtcNow;

        await _context.SaveChangesAsync(
            cancellationToken);

        await _auditService.WriteAsync(
            "UPDATE",
            "workflow_transition",
            transition.workflow_transitions_id,
            oldData,
            new
            {
                transition.workflow_transitions_id,
                transition.workflow_id,
                transition.from_status_id,
                transition.to_status_id
            },
            cancellationToken);

        // НОВОЕ (кэш): переход перенаправлен (например, шаг
        // возврата теперь ведёт не на предыдущий, а на два назад)
        // — допустимые переводы в карточках изменились.
        // workflowId — из загруженной сущности перехода.
        await InvalidateWorkflowAsync(
            transition.workflow_id!.Value,
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var transition =
            await _context.workflow_transitions
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_transitions_id ==
                        id,
                    cancellationToken);

        if (transition is null)
        {
            return false;
        }

        var oldData = new
        {
            transition.workflow_transitions_id,
            transition.workflow_id,
            transition.from_status_id,
            transition.to_status_id
        };

        var workflow =
            await _context.workflows
                .FirstOrDefaultAsync(
                    x =>
                        x.workflows_id ==
                        transition.workflow_id,
                    cancellationToken);

        _context.workflow_transitions.Remove(
            transition);

        if (workflow is not null)
        {
            workflow.version =
                (workflow.version ?? 0) + 1;

            workflow.updated_at =
                DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(
            cancellationToken);

        await _auditService.WriteAsync(
            "DELETE",
            "workflow_transition",
            id,
            oldData,
            null,
            cancellationToken);

        // НОВОЕ (кэш): переход удалён — его больше нельзя
        // предложить в списке «Новый статус». Без инвалидации
        // кэш схемы отдавал бы удалённый переход до истечения
        // TTL, и ChangeStatusAsync отклонил бы выбор пользователя
        // ошибкой «Переход запрещён» — пользователь видел бы
        // доступный в списке, но неработающий вариант.
        await InvalidateWorkflowAsync(
            transition.workflow_id!.Value,
            cancellationToken);

        return true;
    }

    public async Task<bool> IsAllowedAsync(
        int workflowId,
        int fromStatusId,
        int toStatusId,
        CancellationToken cancellationToken)
    {
        // СОЗНАТЕЛЬНО БЕЗ КЭША. Три причины:
        // 1) Метод вызывается внутри InteractionService.
        //    ChangeStatusAsync ВО ВРЕМЯ операции записи.
        //    Кэшированная проверка допустимости дала бы
        //    рассинхрон: админ удалил переход, кэш ещё разрешает —
        //    статус ушёл по запрещённому пути, или наоборот.
        // 2) Это проверка БЕЗОПАСНОСТИ, а не производительности:
        //    один индексный запрос к workflow_transitions
        //    (составной индекс workflow_id + from_status_id +
        //    to_status_id) занимает доли миллисекунды.
        // 3) NFR 1 (отклик до 1 сек) не пострадает: даже при
        //    50 параллельных пользователях это точечные
        //    обращения к проиндексированной таблице рёбер.
        return await _context.workflow_transitions
            .AsNoTracking()
            .AnyAsync(
                x =>
                    x.workflow_id == workflowId &&
                    x.from_status_id == fromStatusId &&
                    x.to_status_id == toStatusId,
                cancellationToken);
    }
}