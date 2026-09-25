using ITSchoolCRM.API.DTOs.Responsible;
using ITSchoolCRM.API.DTOs.Errors;
using ITSchoolCRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSchoolCRM.API.Services.Interfaces;

namespace ITSchoolCRM.API.Controllers;

/// <summary>
/// Раздел «Ответственные»:
///  — менеджеры школы: просмотр — всем авторизованным, закрепление вузов
///    (п. 11 ТЗ: «руководитель … изменяет ответственных пользователей
///    за вузы (менять, удалять, назначать)») — manager/admin;
///  — представители вузов: CRUD каталога university_contacts
///    (требование 1 ТЗ: список ответственных — каталог в БД),
///    удаление — мягкое (is_active = false, 152-ФЗ, аудит).
///
/// Контроллер тонкий: ролевая проверка (из claims Keycloak) и маппинг
/// исключений сервиса в ErrorResponseDto { code, message } — здесь;
/// вся работа с БД — в IResponsiblesService / ResponsiblesService.
///
/// Маршруты:
///   GET    /api/Responsibles/managers
///   PUT    /api/Responsibles/managers/{userId}/universities
///   GET    /api/Responsibles/contacts
///   POST   /api/Responsibles/contacts
///   PUT    /api/Responsibles/contacts/{id}
///   DELETE /api/Responsibles/contacts/{id}
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResponsiblesController : ControllerBase
{
    private readonly IResponsiblesService _service;

    public ResponsiblesController(IResponsiblesService service)
    {
        _service = service;
    }

    // ---------------------------------------------------------------
    // Ролевая модель (п. 11 ТЗ): управление ответственными —
    // привилегия руководителя (manager) и администратора (admin).
    // Роли приходят из Keycloak в claims (realm roles).
    // ---------------------------------------------------------------
    private bool CanManage =>
        User.IsInRole("manager") || User.IsInRole("admin");

    /// <summary>
    /// Текущий пользователь БД (users.users_id) — для audit_logs.user_id.
    /// Резолвим по keycloak_user_id из claim "sub" (стандартный claim
    /// Keycloak; бэкенд записывает его в users.keycloak_user_id при
    /// регистрации/первом входе). Резолв вынесен в сервис,
    /// контроллер DbContext не трогает.
    /// </summary>
    private async Task<int?> GetCurrentUserIdAsync()
    {
        var sub = User.FindFirst("sub")?.Value
                  ?? User.FindFirst("keycloak_user_id")?.Value;
        if (string.IsNullOrEmpty(sub)) return null;

        return await _service.ResolveUserIdByKeycloakSubAsync(sub);
    }

    // ---------------------------------------------------------------
    // GET /api/Responsibles/managers
    // «Менеджеры школы» — всем авторизованным ролям (просмотр).
    // ---------------------------------------------------------------
    [HttpGet("managers")]
    public async Task<ActionResult<List<ResponsibleManagerDto>>> GetManagers(
        CancellationToken ct)
    {
        return await _service.GetManagersAsync(ct);
    }

    // ---------------------------------------------------------------
    // PUT /api/Responsibles/managers/{userId}/universities
    // Полная замена закрепления вузов за менеджером — manager/admin.
    // ---------------------------------------------------------------
    [HttpPut("managers/{userId:int}/universities")]
    public async Task<ActionResult<ResponsibleManagerDto>> UpdateManagerUniversities(
        int userId, UpdateManagerUniversitiesDto dto, CancellationToken ct)
    {
        if (!CanManage)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseDto
            {
                Code = "ERR_FORBIDDEN",
                Message = "Недостаточно прав. Закрепление вузов доступно руководителю и администратору.",
            });
        }

        try
        {
            var actingUserId = await GetCurrentUserIdAsync();
            return await _service.UpdateManagerUniversitiesAsync(
                userId, dto, actingUserId, ct);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponseDto
            {
                Code = "ERR_NOT_FOUND",
                Message = ex.Message,
            });
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(new ErrorResponseDto
            {
                Code = "ERR_VALIDATION",
                Message = ex.Message,
            });
        }
    }

    // ---------------------------------------------------------------
    // GET /api/Responsibles/contacts
    // Каталог представителей вузов — всем авторизованным ролям.
    // ---------------------------------------------------------------
    [HttpGet("contacts")]
    public async Task<ActionResult<List<UniversityContactDto>>> GetContacts(
        CancellationToken ct)
    {
        return await _service.GetContactsAsync(ct);
    }

    // ---------------------------------------------------------------
    // POST /api/Responsibles/contacts
    // Новый представитель вуза — manager/admin.
    // ---------------------------------------------------------------
    [HttpPost("contacts")]
    public async Task<ActionResult<UniversityContactDto>> CreateContact(
        SaveUniversityContactDto dto, CancellationToken ct)
    {
        if (!CanManage)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseDto
            {
                Code = "ERR_FORBIDDEN",
                Message = "Недостаточно прав. Управление представителями доступно руководителю и администратору.",
            });
        }

        try
        {
            var actingUserId = await GetCurrentUserIdAsync();
            var created = await _service.CreateContactAsync(dto, actingUserId, ct);
            return Ok(created);
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(new ErrorResponseDto
            {
                Code = "ERR_VALIDATION",
                Message = ex.Message,
            });
        }
    }

    // ---------------------------------------------------------------
    // PUT /api/Responsibles/contacts/{id}
    // Редактирование представителя (включая активацию/деактивацию)
    // — manager/admin.
    // ---------------------------------------------------------------
    [HttpPut("contacts/{id:int}")]
    public async Task<ActionResult<UniversityContactDto>> UpdateContact(
        int id, SaveUniversityContactDto dto, CancellationToken ct)
    {
        if (!CanManage)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseDto
            {
                Code = "ERR_FORBIDDEN",
                Message = "Недостаточно прав. Управление представителями доступно руководителю и администратору.",
            });
        }

        try
        {
            var actingUserId = await GetCurrentUserIdAsync();
            return await _service.UpdateContactAsync(id, dto, actingUserId, ct);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponseDto
            {
                Code = "ERR_NOT_FOUND",
                Message = ex.Message,
            });
        }
        catch (InvalidDataException ex)
        {
            return BadRequest(new ErrorResponseDto
            {
                Code = "ERR_VALIDATION",
                Message = ex.Message,
            });
        }
    }

    // ---------------------------------------------------------------
    // DELETE /api/Responsibles/contacts/{id}
    // Мягкое удаление (is_active = false) — manager/admin.
    // Идемпотентно: повторный вызов для неактивного контакта — 204.
    // ---------------------------------------------------------------
    [HttpDelete("contacts/{id:int}")]
    public async Task<IActionResult> DeleteContact(int id, CancellationToken ct)
    {
        if (!CanManage)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponseDto
            {
                Code = "ERR_FORBIDDEN",
                Message = "Недостаточно прав. Управление представителями доступно руководителю и администратору.",
            });
        }

        try
        {
            var actingUserId = await GetCurrentUserIdAsync();
            await _service.DeleteContactAsync(id, actingUserId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ErrorResponseDto
            {
                Code = "ERR_NOT_FOUND",
                Message = ex.Message,
            });
        }
    }
}