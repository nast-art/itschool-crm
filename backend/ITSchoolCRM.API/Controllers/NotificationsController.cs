using ITSchoolCRM.API.DTOs.Notifications;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace ITSchoolCRM.API.Controllers;

/// <summary>
/// Уведомления текущего пользователя (п. «уведомления не будут лишним»,
/// 16.09.2026 12:40–12:41). Авторизация — JWT Keycloak (п. 10 ТЗ);
/// пользователь и роли берутся из токена, users.keycloak_user_id —
/// связь с БД CRM.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;

    public NotificationsController(INotificationService notifications)
    {
        _notifications = notifications;
    }

    /// <summary>Уведомления текущего пользователя (по видимости из 152-ФЗ).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetAll(
        CancellationToken ct)
    {
        var (userId, roles) = await ResolveUserAsync(ct);
        if (userId == null)
        {
            return Unauthorized(Error("ERR_UNAUTHORIZED", "Пользователь не найден в CRM."));
        }

        var list = await _notifications.GetForUserAsync(userId.Value, roles, ct);
        return Ok(list);
    }

    /// <summary>Отметить одно уведомление прочитанным.</summary>
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(string id, CancellationToken ct)
    {
        var (userId, _) = await ResolveUserAsync(ct);
        if (userId == null)
        {
            return Unauthorized(Error("ERR_UNAUTHORIZED", "Пользователь не найден в CRM."));
        }

        await _notifications.MarkReadAsync(userId.Value, id, ct);
        return NoContent();
    }

    /// <summary>Отметить все текущие уведомления прочитанными.</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var (userId, roles) = await ResolveUserAsync(ct);
        if (userId == null)
        {
            return Unauthorized(Error("ERR_UNAUTHORIZED", "Пользователь не найден в CRM."));
        }

        // roles передаём в сервис: набор «текущих» уведомлений
        // должен совпадать с тем, что вернул бы GET для этого пользователя
        await _notifications.MarkAllReadAsync(userId.Value, roles, ct);
        return NoContent();
    }

    // ---------- Служебное ----------

    /// <summary>Разрешаем Keycloak (sub + realm_access.roles) в users.users_id.
    /// sub JWT = users.keycloak_user_id (п. 10 ТЗ + сид).</summary>
    private async Task<(int? UserId, IReadOnlyCollection<string> Roles)> ResolveUserAsync(
        CancellationToken ct)
    {
        var keycloakId = User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(keycloakId))
        {
            return (null, Array.Empty<string>());
        }

        var roles = ExtractRoles(User);
        return (await _notifications.ResolveUserIdAsync(keycloakId, ct), roles);
    }

    private static IReadOnlyCollection<string> ExtractRoles(ClaimsPrincipal principal)
    {
        // realm_access: { "roles": ["user","admin"] } — JSON в claim
        var raw = principal.FindFirstValue("realm_access");
        if (string.IsNullOrEmpty(raw)) return Array.Empty<string>();

        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("roles", out var roles) &&
                roles.ValueKind == JsonValueKind.Array)
            {
                return roles.EnumerateArray()
                    .Select(r => r.GetString())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Cast<string>()
                    .ToList();
            }
        }
        catch (JsonException)
        {
            // неожиданный формат claim — считаем, что ролей нет;
            // бэкенд всё равно отдаст только свои данные
        }
        return Array.Empty<string>();
    }

    private static object Error(string code, string message) =>
        new { code, message };
}