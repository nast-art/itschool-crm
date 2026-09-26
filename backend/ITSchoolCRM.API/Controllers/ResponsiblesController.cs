using ITSchoolCRM.API.DTOs.Responsible;
using ITSchoolCRM.API.DTOs.Errors;
using ITSchoolCRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITSchoolCRM.API.Services.Interfaces;

namespace ITSchoolCRM.API.Controllers;

/// <summary>
/// Раздел «Ответственные»:
///  — менеджеры школы: просмотр — всем авторизованным, закрепление вузов — manager/admin;
///  — представители вузов: CRUD каталога university_contacts (список ответственных —
///    каталог в БД), удаление — мягкое (is_active = false, 152-ФЗ, аудит).
///
/// Контроллер тонкий: ролевая проверка (из claims Keycloak) и маппинг исключений
/// сервиса в ErrorResponseDto { code, message } — здесь; вся работа с БД — в
/// IResponsiblesService / ResponsiblesService.
/// </summary>
/// <remarks>
/// Маршруты:
///   GET    /api/Responsibles/managers
///   PUT    /api/Responsibles/managers/{userId}/universities
///   GET    /api/Responsibles/contacts
///   POST   /api/Responsibles/contacts
///   PUT    /api/Responsibles/contacts/{id}
///   DELETE /api/Responsibles/contacts/{id}
/// </remarks>
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

    /// <summary>
    /// Ролевая модель: управление ответственными — привилегия руководителя
    /// (manager) и администратора (admin). Роли приходят из Keycloak в claims (realm roles).
    /// </summary>
    private bool CanManage =>
        User.IsInRole("manager") || User.IsInRole("admin");

    /// <summary>
    /// Текущий пользователь БД (users.users_id) — для audit_logs.user_id.
    /// Резолвим по keycloak_user_id из claim "sub"; резолв вынесен в сервис,
    /// контроллер DbContext не трогает.
    /// </summary>
    private async Task<int?> GetCurrentUserIdAsync()
    {
        var sub = User.FindFirst("sub")?.Value
                  ?? User.FindFirst("keycloak_user_id")?.Value;
        if (string.IsNullOrEmpty(sub))
        {
            return null;
        }

        return await _service.ResolveUserIdByKeycloakSubAsync(sub);
    }

    /// <summary>GET /api/Responsibles/managers — «Менеджеры школы», всем авторизованным ролям.</summary>
    [HttpGet("managers")]
    public async Task<ActionResult<List<ResponsibleManagerDto>>> GetManagers(CancellationToken ct)
    {
        return await _service.GetManagersAsync(ct);
    }

    /// <summary>PUT /api/Responsibles/managers/{userId}/universities — полная замена закрепления вузов за менеджером, manager/admin.</summary>
    [HttpPut("managers/{userId:int}/universities")]
    public async Task<ActionResult<ResponsibleManagerDto>> UpdateManagerUniversities(int userId, UpdateManagerUniversitiesDto dto, CancellationToken ct)
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
            return await _service.UpdateManagerUniversitiesAsync(userId, dto, actingUserId, ct);
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

    /// <summary>GET /api/Responsibles/contacts — каталог представителей вузов, всем авторизованным ролям.</summary>
    [HttpGet("contacts")]
    public async Task<ActionResult<List<UniversityContactDto>>> GetContacts(CancellationToken ct)
    {
        return await _service.GetContactsAsync(ct);
    }

    /// <summary>POST /api/Responsibles/contacts — новый представитель вуза, manager/admin.</summary>
    [HttpPost("contacts")]
    public async Task<ActionResult<UniversityContactDto>> CreateContact(SaveUniversityContactDto dto, CancellationToken ct)
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

    /// <summary>PUT /api/Responsibles/contacts/{id} — редактирование представителя (включая активацию/деактивацию), manager/admin.</summary>
    [HttpPut("contacts/{id:int}")]
    public async Task<ActionResult<UniversityContactDto>> UpdateContact(int id, SaveUniversityContactDto dto, CancellationToken ct)
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

    /// <summary>
    /// DELETE /api/Responsibles/contacts/{id} — мягкое удаление (is_active = false), manager/admin.
    /// Идемпотентно: повторный вызов для неактивного контакта — 204.
    /// </summary>
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