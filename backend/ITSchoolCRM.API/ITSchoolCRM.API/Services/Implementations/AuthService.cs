using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using ITSchoolCRM.API.DTOs.Auth;
using ITSchoolCRM.API.Integrations.Common;
using ITSchoolCRM.API.Services.Interfaces;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Регистрация, вход и обновление токенов через Keycloak.
///
/// ВСЯ валидация входных данных выполняется здесь:
/// некорректные данные -> ArgumentException -> ExceptionHandlingMiddleware
/// вернёт HTTP 400 с кодом ERR_VALIDATION.
///
/// Регистрация (Admin API, сервисный клиент client_credentials):
///   1. POST /realms/{realm}/protocol/openid-connect/token          — сервисный токен
///   2. POST /admin/realms/{realm}/users                            — создание пользователя
///   2а. PUT /admin/realms/{realm}/users/{id}                       — снятие обязательных
///                                                                      действий (required actions)
///   3. PUT  /admin/realms/{realm}/users/{id}/reset-password        — пароль
///   4. GET  /admin/realms/{realm}/roles/{defaultRole}              — роль по умолчанию
///   5. POST /admin/realms/{realm}/users/{id}/role-mappings/realm   — назначение роли
///
/// В Keycloak уходят только штатные поля: firstName = имя, lastName = фамилия.
/// Отчество Keycloak не хранит — оно сохраняется исключительно в CRM
/// (users.middle_name): AuthController передаёт его в UserSyncService напрямую,
/// минуя Keycloak. Паттерн: identity provider хранит минимальный профиль,
/// расширенные атрибуты (отчество) — в приложении.
///
/// Вход и обновление токенов (публичный клиент):
///   POST /realms/{realm}/protocol/openid-connect/token
///   grant_type = password (вход) или grant_type = refresh_token (обновление).
///
/// Секреты сервисного клиента хранятся только на сервере (appsettings.json).
/// Ошибки Keycloak -> IntegrationException -> HTTP 502 с кодом KEYCLOAK_*.
/// Занятый email -> InvalidOperationException -> HTTP 409 ERR_CONFLICT.
/// Неверный логин/пароль или протухший refresh token -> null ->
/// контроллер отдаёт 401 (ERR_INVALID_CREDENTIALS / ERR_SESSION_EXPIRED).
/// </summary>
public class AuthService : IAuthService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    // Допустимые символы в частях ФИО: буквы кириллицы/латиницы и дефис
    // (двойные фамилии вида "Салтыков-Щедрин").
    private const string NameCharsPattern = @"^[A-Za-zА-Яа-яЁё\-]+$";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    private string KeycloakUrl =>
        (_configuration["Keycloak:Url"] ?? string.Empty).TrimEnd('/');

    private string Realm =>
        _configuration["Keycloak:Realm"] ?? string.Empty;

    private string PublicClientId =>
        _configuration["Keycloak:PublicClientId"] ?? string.Empty;

    private string AdminClientId =>
        _configuration["Keycloak:AdminClientId"] ?? string.Empty;

    private string AdminClientSecret =>
        _configuration["Keycloak:AdminClientSecret"] ?? string.Empty;

    private string DefaultRole =>
        _configuration["Keycloak:DefaultRole"] ?? "user";

    // ============================================================
    // РЕГИСТРАЦИЯ
    // ============================================================

    public async Task<string> RegisterAsync(
        RegisterDto dto,
        CancellationToken cancellationToken)
    {
        // ---------- Валидация (вся здесь) ----------
        ValidateRegisterDto(dto);

        var email = dto.Email!.Trim();
        var lastName = dto.LastName!.Trim();
        var firstName = dto.FirstName!.Trim();

        // ---------- Сервисный токен Keycloak ----------
        var serviceToken =
            await GetServiceTokenAsync(cancellationToken);

        using var client =
            _httpClientFactory.CreateClient("Keycloak");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", serviceToken);

        // ---------- 1. Создание пользователя (логин = email) ----------
        // В Keycloak уходят только штатные поля (имя и фамилия).
        // Отчество здесь не передаётся — оно сохраняется в CRM (users.middle_name).
        var createPayload =
            new Dictionary<string, object?>
            {
                ["username"] = email,
                ["email"] = email,
                ["firstName"] = firstName,
                ["lastName"] = lastName,
                ["enabled"] = true,
                ["emailVerified"] = true
            };

        var createResponse =
            await client.PostAsJsonAsync(
                $"{KeycloakUrl}/admin/realms/{Realm}/users",
                createPayload,
                JsonOptions,
                cancellationToken);

        if (createResponse.StatusCode == HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException(
                "Пользователь с таким email уже зарегистрирован.");
        }

        if (!createResponse.IsSuccessStatusCode)
        {
            var body =
                await createResponse.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new IntegrationException(
                "KEYCLOAK_BAD_RESPONSE",
                $"Keycloak вернул код {(int)createResponse.StatusCode} при создании пользователя: {body}");
        }

        // Keycloak возвращает id созданного пользователя в заголовке Location
        var location =
            createResponse.Headers.Location?.ToString()
            ?? throw new IntegrationException(
                "KEYCLOAK_BAD_RESPONSE",
                "Keycloak не вернул идентификатор созданного пользователя.");

        var userId =
            location
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Last();

        // ---------- 2. Установка постоянного пароля ----------
        var passwordResponse =
            await client.PutAsJsonAsync(
                $"{KeycloakUrl}/admin/realms/{Realm}/users/{userId}/reset-password",
                new
                {
                    type = "password",
                    value = dto.Password!,
                    temporary = false
                },
                JsonOptions,
                cancellationToken);

        if (!passwordResponse.IsSuccessStatusCode)
        {
            var body =
                await passwordResponse.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new IntegrationException(
                "KEYCLOAK_BAD_RESPONSE",
                $"Не удалось установить пароль ({(int)passwordResponse.StatusCode}): {body}");
        }

        // ---------- 2а. Снятие обязательных действий (required actions) ----------
        // Keycloak может навесить их автоматически (default-настройки realm'а).
        // Пока required action не выполнено, login невозможен
        // ("Account is not fully set up") — поэтому снимаем принудительно.
        var clearActionsResponse =
            await client.PutAsJsonAsync(
                $"{KeycloakUrl}/admin/realms/{Realm}/users/{userId}",
                new
                {
                    requiredActions = Array.Empty<string>()
                },
                JsonOptions,
                cancellationToken);

        if (!clearActionsResponse.IsSuccessStatusCode)
        {
            var actionsBody =
                await clearActionsResponse.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new IntegrationException(
                "KEYCLOAK_BAD_RESPONSE",
                $"Не удалось снять обязательные действия пользователя ({(int)clearActionsResponse.StatusCode}): {actionsBody}");
        }

        // ---------- 3. Роль по умолчанию "user" ----------
        var roleResponse =
            await client.GetAsync(
                $"{KeycloakUrl}/admin/realms/{Realm}/roles/{DefaultRole}",
                cancellationToken);

        if (!roleResponse.IsSuccessStatusCode)
        {
            throw new IntegrationException(
                "KEYCLOAK_BAD_RESPONSE",
                $"Роль \"{DefaultRole}\" не найдена в Keycloak. Создайте realm-роль.");
        }

        var roleJson =
            await roleResponse.Content.ReadAsStringAsync(
                cancellationToken);

        using var roleDocument =
            JsonDocument.Parse(roleJson);

        var roleId =
            roleDocument.RootElement.GetProperty("id").GetString()
            ?? throw new IntegrationException(
                "KEYCLOAK_BAD_RESPONSE",
                "Keycloak вернул роль без идентификатора.");

        var roleName =
            roleDocument.RootElement.GetProperty("name").GetString()
            ?? DefaultRole;

        // ---------- 4. Назначение роли пользователю ----------
        var assignResponse =
            await client.PostAsJsonAsync(
                $"{KeycloakUrl}/admin/realms/{Realm}/users/{userId}/role-mappings/realm",
                new[]
                {
                    new { id = roleId, name = roleName }
                },
                JsonOptions,
                cancellationToken);

        if (!assignResponse.IsSuccessStatusCode)
        {
            var body =
                await assignResponse.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new IntegrationException(
                "KEYCLOAK_BAD_RESPONSE",
                $"Не удалось назначить роль ({(int)assignResponse.StatusCode}): {body}");
        }

        return userId;
    }

    // ============================================================
    // ВХОД
    // ============================================================

    public async Task<AuthTokenDto?> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken)
    {
        // ---------- Валидация (вся здесь) ----------
        ValidateLoginDto(dto);

        using var body =
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = PublicClientId,
                    ["username"] = dto.UserName!.Trim(),
                    ["password"] = dto.Password!
                });

        var (json, errorDescription) =
            await RequestTokenEndpointAsync(
                body,
                cancellationToken);

        if (json is null)
        {
            if (errorDescription?.Contains("not fully set up") == true)
            {
                throw new IntegrationException(
                    "KEYCLOAK_ACCOUNT_NOT_READY",
                    "Учётная запись ожидает завершения настройки (смены пароля). Обратитесь к администратору.");
            }

            // Неверный логин или пароль: Keycloak отвечает 400 invalid_grant.
            // Контроллер сам отдаст 401 в формате ErrorResponseDto.
            return null;
        }

        return ParseTokenDto(json);
    }

    // ============================================================
    // ОБНОВЛЕНИЕ ТОКЕНОВ (refresh_token grant)
    // ============================================================

    public async Task<AuthTokenDto?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token обязателен.");
        }

        using var body =
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = PublicClientId,
                    ["refresh_token"] = refreshToken
                });

        var (json, _) =
            await RequestTokenEndpointAsync(
                body,
                cancellationToken);

        if (json is null)
        {
            // Refresh token протух или отозван: контроллер отдаст
            // 401 ERR_SESSION_EXPIRED — фронтенд отправит на страницу входа.
            return null;
        }

        return ParseTokenDto(json);
    }

    // ============================================================
    // ВАЛИДАЦИЯ (единственное место всех проверок)
    // ============================================================

    private static void ValidateRegisterDto(RegisterDto dto)
    {
        // ---------- Фамилия ----------
        if (string.IsNullOrWhiteSpace(dto.LastName))
        {
            throw new ArgumentException("Фамилия обязательна.");
        }

        var lastName = dto.LastName.Trim();

        if (lastName.Length < 2 || lastName.Length > 100)
        {
            throw new ArgumentException("Фамилия должна содержать от 2 до 100 символов.");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(
                lastName,
                NameCharsPattern))
        {
            throw new ArgumentException(
                "Фамилия может содержать только буквы (русские/латинские) и дефис.");
        }

        // ---------- Имя ----------
        if (string.IsNullOrWhiteSpace(dto.FirstName))
        {
            throw new ArgumentException("Имя обязательно.");
        }

        var firstName = dto.FirstName.Trim();

        if (firstName.Length < 2 || firstName.Length > 100)
        {
            throw new ArgumentException("Имя должно содержать от 2 до 100 символов.");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(
                firstName,
                NameCharsPattern))
        {
            throw new ArgumentException(
                "Имя может содержать только буквы (русские/латинские) и дефис.");
        }

        // ---------- Отчество (необязательно; хранится только в CRM) ----------
        if (!string.IsNullOrWhiteSpace(dto.MiddleName))
        {
            var middleName = dto.MiddleName.Trim();

            if (middleName.Length < 2 || middleName.Length > 100)
            {
                throw new ArgumentException("Отчество должно содержать от 2 до 100 символов.");
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(
                    middleName,
                    NameCharsPattern))
            {
                throw new ArgumentException(
                    "Отчество может содержать только буквы (русские/латинские) и дефис.");
            }
        }

        // ---------- Email ----------
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new ArgumentException("Email обязателен.");
        }

        var email = dto.Email.Trim();

        if (email.Length > 200 ||
            !email.Contains('@') ||
            email.StartsWith('@') ||
            email.EndsWith('@'))
        {
            throw new ArgumentException("Некорректный формат email.");
        }

        // ---------- Пароль ----------
        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Пароль обязателен.");
        }

        if (dto.Password.Length < 8)
        {
            throw new ArgumentException("Пароль должен содержать не менее 8 символов.");
        }

        if (dto.Password.Length > 100)
        {
            throw new ArgumentException("Пароль не должен превышать 100 символов.");
        }
    }

    private static void ValidateLoginDto(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName))
        {
            throw new ArgumentException("Логин обязателен.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Пароль обязателен.");
        }
    }

    // ============================================================
    // ОБЩИЕ ХЕЛПЕРЫ KEYCLOAK
    // ============================================================

    /// <summary>
    /// Обращение к token-endpoint Keycloak. При успехе возвращает тело ответа
    /// и null в поле ошибки; при ошибке гранта — null и error_description
    /// из ответа Keycloak. Сетевые сбои -> IntegrationException
    /// (502 KEYCLOAK_UNAVAILABLE).
    /// </summary>
    private async Task<(string? Json, string? ErrorDescription)>
        RequestTokenEndpointAsync(
            FormUrlEncodedContent body,
            CancellationToken cancellationToken)
    {
        using var client =
            _httpClientFactory.CreateClient("Keycloak");

        HttpResponseMessage response;

        try
        {
            response =
                await client.PostAsync(
                    $"{KeycloakUrl}/realms/{Realm}/protocol/openid-connect/token",
                    body,
                    cancellationToken);
        }
        catch (Exception ex)
            when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new IntegrationException(
                "KEYCLOAK_UNAVAILABLE",
                "Keycloak временно недоступен.",
                ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            // 400 invalid_grant (неверный логин/пароль, протухший refresh token)
            // и прочие ошибки гранта: сообщаем контроллеру через null,
            // чтобы тот вернул осмысленный 401. error_description пригодится
            // для различения частных случаев (например, "not fully set up").
            var errorJson =
                await response.Content.ReadAsStringAsync(cancellationToken);

            string? description = null;

            try
            {
                using var errorDoc =
                    JsonDocument.Parse(errorJson);

                if (errorDoc.RootElement.TryGetProperty(
                        "error_description",
                        out var descElement))
                {
                    description = descElement.GetString();
                }
            }
            catch (JsonException)
            {
                // тело ошибки не JSON — оставляем description = null
            }

            return (null, description);
        }

        var json =
            await response.Content.ReadAsStringAsync(cancellationToken);

        return (json, null);
    }

    /// <summary>
    /// Разбор ответа token-endpoint ({ access_token, refresh_token, expires_in })
    /// в AuthTokenDto. Бросает IntegrationException при неполном ответе.
    /// </summary>
    private static AuthTokenDto ParseTokenDto(string json)
    {
        using var document =
            JsonDocument.Parse(json);

        var root = document.RootElement;

        return new AuthTokenDto
        {
            AccessToken =
                root.GetProperty("access_token").GetString(),

            RefreshToken =
                root.TryGetProperty("refresh_token", out var refresh)
                    ? refresh.GetString()
                    : null,

            ExpiresIn =
                root.TryGetProperty("expires_in", out var expires)
                    ? expires.GetInt32()
                    : 0
        };
    }

    // ============================================================
    // СЕРВИСНЫЙ ТОКЕН KEYCLOAK (client_credentials)
    // ============================================================

    private async Task<string> GetServiceTokenAsync(
        CancellationToken cancellationToken)
    {
        using var client =
            _httpClientFactory.CreateClient("Keycloak");

        using var body =
            new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = AdminClientId,
                    ["client_secret"] = AdminClientSecret
                });

        HttpResponseMessage response;

        try
        {
            response =
                await client.PostAsync(
                    $"{KeycloakUrl}/realms/{Realm}/protocol/openid-connect/token",
                    body,
                    cancellationToken);
        }
        catch (Exception ex)
            when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new IntegrationException(
                "KEYCLOAK_UNAVAILABLE",
                "Keycloak временно недоступен.",
                ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var text =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            throw new IntegrationException(
                "KEYCLOAK_BAD_RESPONSE",
                $"Keycloak вернул код {(int)response.StatusCode} при получении сервисного токена: {text}");
        }

        var json =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        using var document =
            JsonDocument.Parse(json);

        return document.RootElement.GetProperty("access_token").GetString()
            ?? throw new IntegrationException(
                "KEYCLOAK_BAD_RESPONSE",
                "Keycloak не вернул сервисный токен.");
    }
}