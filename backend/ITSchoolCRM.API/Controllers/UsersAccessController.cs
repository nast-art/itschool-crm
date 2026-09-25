using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.AccessControl;                    // CrmDbContext
using ITSchoolCRM.API.DTOs.Errors;               // ErrorResponseDto (существующий)
using ITSchoolCRM.API.Models;                    // user, audit_log, UserAccessItemDto, UpdateUserAccessRequest
using ITSchoolCRM.API.Services.Implementations;  // KeycloakUserRecord, KeycloakUnavailableException
using ITSchoolCRM.API.Services.Interfaces;       // IKeycloakAdminClient
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ITSchoolCRM.API.Controllers
{
    /// <summary>
    /// Управление пользователями и правами доступа (п. 11 ТЗ).
    /// Только роль admin. Не пересекается с существующим
    /// UsersController: добавляет маршруты access-control.
    ///
    /// GET  /api/Users/access-control                    — список с ролями
    /// PUT  /api/Users/{keycloakUserId}/access-control   — смена роли/статуса
    /// </summary>
    [ApiController]
    [Route("api/Users")]
    [Authorize(Roles = "admin")]
    public sealed class UsersAccessController : ControllerBase
    {
        private static readonly string[] AppRoles = ["user", "manager", "admin"];

        private static readonly JsonSerializerOptions AuditJsonOptions = new()
        {
            WriteIndented = false,
        };

        private readonly CrmDbContext _db;
        private readonly IKeycloakAdminClient _keycloak;
        private readonly ILogger<UsersAccessController> _logger;

        public UsersAccessController(
            CrmDbContext db,
            IKeycloakAdminClient keycloak,
            ILogger<UsersAccessController> logger)
        {
            _db = db;
            _keycloak = keycloak;
            _logger = logger;
        }

        // ---------- GET /api/Users/access-control ----------

        /// <summary>
        /// Список пользователей с ролями и статусом.
        /// Источник истины по роли — Keycloak; по активности — users.is_active.
        /// Локальные записи upsert-ятся по keycloak_user_id.
        /// </summary>
        [HttpGet("access-control")]
        [ProducesResponseType(typeof(List<UserAccessItemDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<List<UserAccessItemDto>>> GetAccessControl(
            CancellationToken ct)
        {
            IReadOnlyList<KeycloakUserRecord> keycloakUsers;
            try
            {
                keycloakUsers = await _keycloak.ListUsersWithRolesAsync(ct);
            }
            catch (KeycloakUnavailableException ex)
            {
                _logger.LogWarning(ex, "GET access-control: Keycloak недоступен");
                return StatusCode(StatusCodes.Status502BadGateway,
                    new ErrorResponseDto
                    {
                        Code = "KEYCLOAK_UNAVAILABLE",
                        Message = ex.Message,
                    });
            }

            // Upsert локальных профилей: связываем Keycloak-пользователей
            // со строками таблицы users (это же нужно фильтрам «Ответственный»)
            var keycloakIds = keycloakUsers.Select(k => k.Id).ToList();
            var localsByKeycloakId = await _db.users
                .Where(u => keycloakIds.Contains(u.keycloak_user_id))
                .ToDictionaryAsync(u => u.keycloak_user_id, ct);

            foreach (var kc in keycloakUsers)
            {
                if (localsByKeycloakId.TryGetValue(kc.Id, out var local))
                {
                    var changed =
                        local.last_name != kc.LastName ||
                        local.first_name != kc.FirstName ||
                        local.middle_name != kc.MiddleName ||
                        local.email != kc.Email;

                    if (changed)
                    {
                        local.last_name = kc.LastName;
                        local.first_name = kc.FirstName;
                        local.middle_name = kc.MiddleName;
                        local.email = kc.Email;
                        local.updated_at = DateTime.UtcNow;
                    }
                }
                else
                {
                    var entity = new user
                    {
                        keycloak_user_id = kc.Id,
                        last_name = kc.LastName,
                        first_name = kc.FirstName,
                        middle_name = kc.MiddleName,
                        email = kc.Email,
                        is_active = true, // при первой синхронизации — активен
                        created_at = DateTime.UtcNow,
                    };
                    _db.users.Add(entity);
                    localsByKeycloakId[kc.Id] = entity;
                }
            }

            await _db.SaveChangesAsync(ct);

            var items = keycloakUsers
                .Select(kc =>
                {
                    var local = localsByKeycloakId[kc.Id];
                    var role = AppRoles.Contains(kc.Role) ? kc.Role : "user";
                    return new UserAccessItemDto
                    {
                        KeycloakUserId = kc.Id,
                        UserName = kc.UserName,
                        Email = kc.Email,
                        LastName = kc.LastName,
                        FirstName = kc.FirstName,
                        MiddleName = kc.MiddleName,
                        FullName = ComposeFullName(kc),
                        Role = role,
                        IsActive = local.is_active ?? true,
                    };
                })
                .OrderBy(i => i.FullName,
                    StringComparer.Create(
                        CultureInfo.GetCultureInfo("ru-RU"),
                        ignoreCase: true))
                .ToList();

            return Ok(items);
        }

        // ---------- PUT /api/Users/{keycloakUserId}/access-control ----------

        /// <summary>
        /// Смена роли и/или блокировка пользователя.
        /// Запрещено: менять собственную запись; лишать последнего
        /// активного администратора роли или блокировать его.
        /// Все изменения фиксируются в audit_logs.
        /// </summary>
        [HttpPut("{keycloakUserId}/access-control")]
        [ProducesResponseType(typeof(UserAccessItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status502BadGateway)]
        public async Task<ActionResult<UserAccessItemDto>> UpdateAccessControl(
            string keycloakUserId,
            [FromBody] UpdateUserAccessRequest request,
            CancellationToken ct)
        {
            // Валидация Role ([RegularExpression]) отрабатывает автоматически
            // через ApiController — сюда попадём только с корректной ролью.

            // 1) Запрет на редактирование собственной записи
            var callerKeycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                   ?? User.FindFirst("sub")?.Value;
            if (callerKeycloakId == keycloakUserId)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new ErrorResponseDto
                    {
                        Code = "ERR_FORBIDDEN",
                        Message = "Нельзя изменить права собственной учётной записи.",
                    });
            }

            // 2) Пользователь должен существовать в Keycloak
            IReadOnlyList<KeycloakUserRecord> keycloakUsers;
            try
            {
                keycloakUsers = await _keycloak.ListUsersWithRolesAsync(ct);
            }
            catch (KeycloakUnavailableException ex)
            {
                _logger.LogWarning(ex,
                    "PUT access-control: Keycloak недоступен при чтении пользователей");
                return StatusCode(StatusCodes.Status502BadGateway,
                    new ErrorResponseDto
                    {
                        Code = "KEYCLOAK_UNAVAILABLE",
                        Message = ex.Message,
                    });
            }

            var target = keycloakUsers.FirstOrDefault(u => u.Id == keycloakUserId);
            if (target is null)
            {
                return NotFound(new ErrorResponseDto
                {
                    Code = "ERR_NOT_FOUND",
                    Message = "Пользователь не найден.",
                });
            }

            // 3) Локальная запись (создаём, если GET ещё не вызывался)
            var local = await _db.users
                .FirstOrDefaultAsync(u => u.keycloak_user_id == keycloakUserId, ct);
            if (local is null)
            {
                local = new user
                {
                    keycloak_user_id = keycloakUserId,
                    last_name = target.LastName,
                    first_name = target.FirstName,
                    middle_name = target.MiddleName,
                    email = target.Email,
                    is_active = true,
                    created_at = DateTime.UtcNow,
                };
                _db.users.Add(local);
            }

            var targetRole = AppRoles.Contains(target.Role) ? target.Role : "user";

            // 4) Защита последнего активного администратора
            var demotingLastAdmin =
           targetRole == "admin" && local.is_active == true &&
           (request.Role != "admin" || !request.IsActive);
            if (demotingLastAdmin)
            {
                var otherActiveAdminKeycloakIds = keycloakUsers
                    .Where(u => u.Role == "admin" && u.Id != keycloakUserId)
                    .Select(u => u.Id)
                    .ToList();

                var otherActiveAdmins = await _db.users
                    .CountAsync(u =>
                        otherActiveAdminKeycloakIds.Contains(u.keycloak_user_id) &&
                        u.is_active == true, ct);

                if (otherActiveAdmins == 0)
                {
                    return Conflict(new ErrorResponseDto
                    {
                        Code = "ERR_CONFLICT",
                        Message = "Нельзя изменить роль или заблокировать последнего " +
                                  "активного администратора.",
                    });
                }
            }

            // 5) Снимок «было» для аудита
            var oldData = new
            {
                role = targetRole,
                isActive = local.is_active,
            };

            // 6) Применяем изменения: роль — в Keycloak, активность — везде
            try
            {
                if (request.Role != targetRole)
                {
                    await _keycloak.UpdateUserRoleAsync(keycloakUserId, request.Role, ct);
                }
                if (request.IsActive != local.is_active)
                {
                    await _keycloak.SetUserEnabledAsync(
                        keycloakUserId, request.IsActive, ct);
                }
            }
            catch (KeycloakUnavailableException ex)
            {
                _logger.LogWarning(ex,
                    "PUT access-control: Keycloak отклонил изменение для {UserId}",
                    keycloakUserId);
                return StatusCode(StatusCodes.Status502BadGateway,
                    new ErrorResponseDto
                    {
                        Code = "KEYCLOAK_UNAVAILABLE",
                        Message = ex.Message,
                    });
            }

            local.is_active = request.IsActive;
            local.updated_at = DateTime.UtcNow;

            // 7) Аудит: кто, что, старое/новое значение (audit_logs, ТЗ ИБ).
            // old_data/new_data — jsonb, скаффолдятся как JsonDocument.
            var callerLocal = await _db.users
                .FirstOrDefaultAsync(u => u.keycloak_user_id == callerKeycloakId, ct);

            var newData = new
            {
                role = request.Role,
                isActive = request.IsActive,
            };

            _db.audit_logs.Add(new audit_log
            {
                user_id = callerLocal?.users_id,
                action = "UPDATE_USER_ACCESS",
                entity_type = "users",
                entity_id = local.users_id,
                old_data = JsonSerializer.Serialize(oldData, AuditJsonOptions),
                new_data = JsonSerializer.Serialize(newData, AuditJsonOptions),
                created_at = DateTime.UtcNow,
            });

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Права пользователя {KeycloakUserId} изменены админом {AdminId}: " +
                "роль {OldRole} -> {NewRole}, активен {OldActive} -> {NewActive}",
                keycloakUserId, callerKeycloakId,
                targetRole, request.Role, local.is_active, request.IsActive);

            // 8) Возвращаем обновлённую запись в формате списка
            var updated = new UserAccessItemDto
            {
                KeycloakUserId = target.Id,
                UserName = target.UserName,
                Email = target.Email,
                LastName = target.LastName,
                FirstName = target.FirstName,
                MiddleName = target.MiddleName,
                FullName = ComposeFullName(target),
                Role = request.Role,
                IsActive = request.IsActive,
            };

            return Ok(updated);
        }

        // ---------- Хелперы ----------

        private static string ComposeFullName(KeycloakUserRecord user)
        {
            var composed = string.Join(' ', new[]
                {
                    user.LastName, user.FirstName, user.MiddleName,
                }.Where(p => !string.IsNullOrWhiteSpace(p)))
                .Trim();
            return composed.Length > 0 ? composed
                : user.UserName ?? user.Email ?? "Без имени";
        }
    }
}