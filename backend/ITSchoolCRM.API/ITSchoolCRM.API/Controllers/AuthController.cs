using ITSchoolCRM.API.DTOs.Auth;
using ITSchoolCRM.API.DTOs.Errors;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthService _authService;
    private readonly IUserSyncService _userSyncService;

    public AuthController(
        ICurrentUserService currentUserService,
        IAuthService authService,
        IUserSyncService userSyncService)
    {
        _currentUserService = currentUserService;
        _authService = authService;
        _userSyncService = userSyncService;
    }

    /// <summary>
    /// Профиль текущего авторизованного пользователя (данные из JWT Keycloak).
    /// Заодно создаёт запись в таблице users, если её ещё нет — так
    /// пользователи, заведённые вручную в консоли Keycloak (admin, manager1...),
    /// автоматически попадают в БД CRM при первом обращении.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(
        CancellationToken cancellationToken)
    {
        await _userSyncService.EnsureCurrentUserAsync(
            cancellationToken);

        return Ok(new
        {
            isAuthenticated = _currentUserService.IsAuthenticated,
            keycloakUserId = _currentUserService.KeycloakUserId,
            userName = _currentUserService.UserName,
            email = _currentUserService.Email,
            fullName = _currentUserService.FullName,
            roles = _currentUserService.Roles
        });
    }

    /// <summary>
    /// Самостоятельная регистрация. Пользователь создаётся в Keycloak
    /// (логин = email, firstName = имя, lastName = фамилия), ему автоматически
    /// назначается роль по умолчанию. После создания в Keycloak сразу
    /// синхронизируется в таблицу users CRM (включая отчество, которое
    /// Keycloak не хранит).
    /// Валидация — в AuthService (400 ERR_VALIDATION),
    /// занятый email — 409 ERR_CONFLICT, сбои Keycloak — 502 KEYCLOAK_*.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
    {
        var keycloakUserId =
            await _authService.RegisterAsync(
                dto,
                cancellationToken);

        // Копия пользователя в таблице users CRM (нужна для
        // interactions.manager_id, university_managers.user_id и отчётов).
        // full_name в таблице отсутствует — храним только части ФИО.
        await _userSyncService.UpsertAsync(
            keycloakUserId,
            dto.LastName,
            dto.FirstName,
            dto.MiddleName,
            dto.Email,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                keycloakUserId,
                email = dto.Email
            });
    }

    /// <summary>
    /// Вход: обмен логина и пароля на JWT Keycloak (password grant).
    /// Неверные учётные данные — 401 ERR_INVALID_CREDENTIALS.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        var tokens =
            await _authService.LoginAsync(
                dto,
                cancellationToken);

        if (tokens is null)
        {
            return Unauthorized(
                new ErrorResponseDto
                {
                    Code = "ERR_INVALID_CREDENTIALS",
                    Message = "Неверный логин или пароль."
                });
        }

        return Ok(tokens);
    }

    /// <summary>
    /// Обновление пары токенов по refresh token (refresh_token grant).
    /// Фронтенд вызывает автоматически при получении 401 от защищённых
    /// эндпоинтов. Протухший/отозванный refresh token — 401 ERR_SESSION_EXPIRED
    /// (тогда фронтенд перенаправляет на страницу входа).
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenDto dto,
        CancellationToken cancellationToken)
    {
        var tokens =
            await _authService.RefreshAsync(
                dto.RefreshToken ?? string.Empty,
                cancellationToken);

        if (tokens is null)
        {
            return Unauthorized(
                new ErrorResponseDto
                {
                    Code = "ERR_SESSION_EXPIRED",
                    Message = "Сессия истекла. Войдите в систему снова."
                });
        }

        return Ok(tokens);
    }
}