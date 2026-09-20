using ITSchoolCRM.API.DTOs.WorkflowStatuses;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IWorkflowStatusService
{
    Task<List<WorkflowStatusDto>> GetByWorkflowIdAsync(
        int workflowId,
        CancellationToken cancellationToken);

    Task<WorkflowStatusDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<WorkflowStatusDto?> CreateAsync(
        CreateWorkflowStatusDto dto,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        int id,
        UpdateWorkflowStatusDto dto,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken);
}