using ITSchoolCRM.API.DTOs.WorkflowTransitions;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IWorkflowTransitionService
{
    Task<List<WorkflowTransitionDto>> GetByWorkflowIdAsync(
        int workflowId,
        CancellationToken cancellationToken);

    Task<WorkflowTransitionDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<WorkflowTransitionDto?> CreateAsync(
        CreateWorkflowTransitionDto dto,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        int id,
        UpdateWorkflowTransitionDto dto,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken);

    Task<bool> IsAllowedAsync(
        int workflowId,
        int fromStatusId,
        int toStatusId,
        CancellationToken cancellationToken);
}