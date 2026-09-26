using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITSchoolCRM.API.DTOs.Workflows;

namespace ITSchoolCRM.API.Services.Interfaces
{
    public interface IWorkflowService
    {
        Task<List<WorkflowDto>> GetAllAsync(CancellationToken cancellationToken);

        Task<WorkflowDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

        Task<List<WorkflowDto>> GetActiveAsync(CancellationToken cancellationToken);

        Task<WorkflowDto> CreateAsync(CreateWorkflowDto dto, CancellationToken cancellationToken);

        Task<bool> UpdateAsync(int id, UpdateWorkflowDto dto, CancellationToken cancellationToken);

        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
    }
}