using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.WorkflowStatuses;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class WorkflowStatusService : IWorkflowStatusService
{
    private readonly CrmDbContext _context;
    private readonly IAuditService _auditService;

    public WorkflowStatusService(
        CrmDbContext context,
        IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<WorkflowStatusDto>> GetByWorkflowIdAsync(
        int workflowId,
        CancellationToken cancellationToken)
    {
        return await _context.workflow_statuses
            .AsNoTracking()
            .Where(x => x.workflow_id == workflowId)
            .OrderBy(x => x.sort_order)
            .Select(x => new WorkflowStatusDto
            {
                Id = x.workflow_statuses_id,
                WorkflowId = x.workflow_id,
                Name = x.name,
                Description = x.description,
                SortOrder = x.sort_order,
                IsInitial = x.is_initial,
                IsFinal = x.is_final
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowStatusDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.workflow_statuses
            .AsNoTracking()
            .Where(x => x.workflow_statuses_id == id)
            .Select(x => new WorkflowStatusDto
            {
                Id = x.workflow_statuses_id,
                WorkflowId = x.workflow_id,
                Name = x.name,
                Description = x.description,
                SortOrder = x.sort_order,
                IsInitial = x.is_initial,
                IsFinal = x.is_final
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<WorkflowStatusDto?> CreateAsync(
        CreateWorkflowStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!dto.WorkflowId.HasValue)
        {
            throw new ArgumentException(
                "WorkflowId обязателен.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException(
                "Название статуса обязательно.");
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

        if (dto.IsInitial == true)
        {
            var existingInitial =
                await _context.workflow_statuses
                    .AnyAsync(
                        x =>
                            x.workflow_id ==
                                dto.WorkflowId.Value &&
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

        workflow.version =
            (workflow.version ?? 0) + 1;

        workflow.updated_at =
            DateTime.UtcNow;

        await _context.SaveChangesAsync(
            cancellationToken);

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
        var status =
            await _context.workflow_statuses
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_statuses_id == id,
                    cancellationToken);

        if (status is null)
        {
            return false;
        }

        if (dto.IsInitial == true)
        {
            var anotherInitial =
                await _context.workflow_statuses
                    .AnyAsync(
                        x =>
                            x.workflow_id ==
                                status.workflow_id &&
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

        var workflow =
            await _context.workflows
                .FirstOrDefaultAsync(
                    x =>
                        x.workflows_id ==
                        status.workflow_id,
                    cancellationToken);

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

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var status =
            await _context.workflow_statuses
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_statuses_id == id,
                    cancellationToken);

        if (status is null)
        {
            return false;
        }

        var used =
            await _context.interactions
                .AnyAsync(
                    x =>
                        x.current_status_id == id,
                    cancellationToken);

        if (used)
        {
            throw new InvalidOperationException(
                "Нельзя удалить статус, который используется interaction.");
        }

        var hasHistory =
            await _context.interaction_status_histories
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

        var hasTransitions =
            await _context.workflow_transitions
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

        var workflow =
            await _context.workflows
                .FirstOrDefaultAsync(
                    x =>
                        x.workflows_id ==
                        status.workflow_id,
                    cancellationToken);

        _context.workflow_statuses.Remove(status);

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
            "workflow_status",
            id,
            oldData,
            null,
            cancellationToken);

        return true;
    }
}