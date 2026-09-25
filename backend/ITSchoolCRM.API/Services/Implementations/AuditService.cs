using System.Text.Json;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Audit;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly CrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuditService(
        CrmDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task WriteAsync(
        string action,
        string entityType,
        int? entityId,
        object? oldData,
        object? newData,
        CancellationToken cancellationToken)
    {
        var userId =
            await _context.users
                .Where(x =>
                    x.keycloak_user_id ==
                    _currentUserService.KeycloakUserId)
                .Select(x => (int?)x.users_id)
                .FirstOrDefaultAsync(
                    cancellationToken);

        var auditLog = new audit_log
        {
            user_id = userId,
            action = action,
            entity_type = entityType,
            entity_id = entityId,

            old_data = oldData is null
                ? null
                : JsonSerializer.Serialize(oldData),

            new_data = newData is null
                ? null
                : JsonSerializer.Serialize(newData),

            created_at = DateTime.UtcNow
        };

        _context.audit_logs.Add(auditLog);

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<List<AuditLogDto>> GetByEntityAsync(
        string entityType,
        int entityId,
        CancellationToken cancellationToken)
    {
        return await _context.audit_logs
            .AsNoTracking()
            .Where(x =>
                x.entity_type == entityType &&
                x.entity_id == entityId)
            .OrderByDescending(x => x.created_at)
            .Select(x => new AuditLogDto
            {
                Id = x.audit_logs_id,

                UserId = x.user_id,

                // Колонки full_name нет: ФИО собирается из частей.
                UserName = x.user == null
                    ? null
                    : x.user.middle_name == null
                        ? x.user.last_name + " " + x.user.first_name
                        : x.user.last_name + " " + x.user.first_name + " " + x.user.middle_name,

                Action = x.action,

                EntityType = x.entity_type,

                EntityId = x.entity_id,

                OldData = x.old_data,

                NewData = x.new_data,

                CreatedAt = x.created_at
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditLogDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.audit_logs
            .AsNoTracking()
            .OrderByDescending(x => x.created_at)
            .Select(x => new AuditLogDto
            {
                Id = x.audit_logs_id,

                UserId = x.user_id,

                // Колонки full_name нет: ФИО собирается из частей.
                UserName = x.user == null
                    ? null
                    : x.user.middle_name == null
                        ? x.user.last_name + " " + x.user.first_name
                        : x.user.last_name + " " + x.user.first_name + " " + x.user.middle_name,

                Action = x.action,

                EntityType = x.entity_type,

                EntityId = x.entity_id,

                OldData = x.old_data,

                NewData = x.new_data,

                CreatedAt = x.created_at
            })
            .ToListAsync(cancellationToken);
    }
}