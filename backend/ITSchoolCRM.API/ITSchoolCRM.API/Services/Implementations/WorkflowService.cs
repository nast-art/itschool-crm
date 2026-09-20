using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.WorkflowStatuses;
using ITSchoolCRM.API.DTOs.WorkflowTransitions;
using ITSchoolCRM.API.DTOs.Workflows;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class WorkflowService : IWorkflowService
{
    private readonly CrmDbContext _context;
    private readonly IAuditService _auditService;

    public WorkflowService(
        CrmDbContext context,
        IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<WorkflowDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.workflows
            .AsNoTracking()
            .OrderBy(x => x.name)
            .Select(x => new WorkflowDto
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
                        FromStatusName =
                            t.from_status != null
                                ? t.from_status.name
                                : null,
                        ToStatusId = t.to_status_id,
                        ToStatusName =
                            t.to_status != null
                                ? t.to_status.name
                                : null
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.workflows
            .AsNoTracking()
            .Where(x => x.workflows_id == id)
            .Select(x => new WorkflowDto
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
                        FromStatusName =
                            t.from_status != null
                                ? t.from_status.name
                                : null,
                        ToStatusId = t.to_status_id,
                        ToStatusName =
                            t.to_status != null
                                ? t.to_status.name
                                : null
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<WorkflowDto>> GetActiveAsync(
        CancellationToken cancellationToken)
    {
        return await _context.workflows
            .AsNoTracking()
            .Where(x => x.is_active == true)
            .OrderBy(x => x.name)
            .Select(x => new WorkflowDto
            {
                Id = x.workflows_id,
                Name = x.name,
                Description = x.description,
                IsActive = x.is_active,
                Version = x.version
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkflowDto> CreateAsync(
        CreateWorkflowDto dto,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException(
                "Название workflow обязательно.");
        }

        var exists =
            await _context.workflows.AnyAsync(
                x => x.name == dto.Name,
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "Workflow с таким названием уже существует.");
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

        await _context.SaveChangesAsync(
            cancellationToken);

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

        return new WorkflowDto
        {
            Id = workflow.workflows_id,
            Name = workflow.name,
            Description = workflow.description,
            IsActive = workflow.is_active,
            Version = workflow.version
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateWorkflowDto dto,
        CancellationToken cancellationToken)
    {
        var workflow =
            await _context.workflows
                .FirstOrDefaultAsync(
                    x => x.workflows_id == id,
                    cancellationToken);

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

        workflow.version =
            (workflow.version ?? 0) + 1;

        workflow.updated_at =
            DateTime.UtcNow;

        await _context.SaveChangesAsync(
            cancellationToken);

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

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var workflow =
            await _context.workflows
                .FirstOrDefaultAsync(
                    x => x.workflows_id == id,
                    cancellationToken);

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

        workflow.is_active = false;
        workflow.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(
            cancellationToken);

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

        return true;
    }
}