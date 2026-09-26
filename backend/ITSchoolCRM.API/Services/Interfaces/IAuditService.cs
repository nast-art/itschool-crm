using ITSchoolCRM.API.DTOs.Audit;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IAuditService
{
    Task WriteAsync(string action, string entityType, int? entityId, object? oldData, object? newData, CancellationToken cancellationToken);

    Task<List<AuditLogDto>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken);

    Task<List<AuditLogDto>> GetAllAsync(CancellationToken cancellationToken);
}