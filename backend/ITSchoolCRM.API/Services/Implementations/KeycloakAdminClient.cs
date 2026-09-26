using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ITSchoolCRM.API.Services.Implementations;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    /// <summary>Базовый URL Keycloak.</summary>
    public string BaseUrl { get; set; } = "http://localhost:8080";

    /// <summary>Realm CRM.</summary>
    public string Realm { get; set; } = "itschool";

    /// <summary>
    /// Клиент с включённым «Service accounts roles» и ролями
    /// realm-management: view-users, manage-users.
    /// </summary>
    public string AdminClientId { get; set; } = "itschool-admin";

    public string AdminClientSecret { get; set; } = string.Empty;
}

/// <summary>Пользователь Keycloak с его realm-ролью CRM.</summary>
public sealed record KeycloakUserRecord(
    string Id,
    string? UserName,
    string? Email,
    string? LastName,
    string? FirstName,
    string? MiddleName,
    string Role);

/// <summary>Keycloak недоступен — фронтенду уйдёт 502 KEYCLOAK_UNAVAILABLE.</summary>
public sealed class KeycloakUnavailableException : Exception
{
    public KeycloakUnavailableException(string message, Exception? inner = null)
        : base(message, inner) { }
}

/// <summary>
/// Клиент Keycloak Admin REST API (client credentials).
/// Реализация <see cref="IKeycloakAdminClient"/>.
/// </summary>
public sealed class KeycloakAdminClient : IKeycloakAdminClient
{
    private static readonly string[] AppRoles = ["user", "manager", "admin"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _http;
    private readonly KeycloakOptions _opt;
    private readonly ILogger<KeycloakAdminClient> _logger;

    // Токен сервисного аккаунта кэшируем до истечения (с запасом 30 с).
    // SemaphoreSlim защищает от параллельного запроса токена
    // из нескольких потоков (клиент — singleton).
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _token;
    private DateTime _tokenExpiresAt = DateTime.MinValue;

    public KeycloakAdminClient(
        HttpClient http,
        IOptions<KeycloakOptions> options,
        ILogger<KeycloakAdminClient> logger)
    {
        _opt = options.Value;
        _logger = logger;
        _http = http;
        _http.BaseAddress = new Uri(_opt.BaseUrl.TrimEnd('/') + "/");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<KeycloakUserRecord>> ListUsersWithRolesAsync(CancellationToken ct)
    {
        using var usersRequest = await CreateRequestAsync(
            HttpMethod.Get,
            $"/admin/realms/{_opt.Realm}/users?max=1000&briefRepresentation=true", ct);
        using var usersResponse = await _http.SendAsync(usersRequest, ct);
        await EnsureSuccessAsync(usersResponse, "список пользователей", ct);

        var users = await usersResponse.Content.ReadFromJsonAsync<List<KcUserBrief>>(ct) ?? [];

        var roleByUserId = new Dictionary<string, string>();

        foreach (var role in AppRoles)
        {
            using var roleRequest = await CreateRequestAsync(
                HttpMethod.Get,
                $"/admin/realms/{_opt.Realm}/roles/{role}/users?max=1000", ct);
            using var roleResponse = await _http.SendAsync(roleRequest, ct);
            await EnsureSuccessAsync(roleResponse, $"пользователи роли {role}", ct);

            var members = await roleResponse.Content.ReadFromJsonAsync<List<KcUserBrief>>(ct) ?? [];

            foreach (var member in members)
            {
                // Не перезаписываем более привилегированную роль:
                // пользователь может входить в несколько групп ролей.
                if (!roleByUserId.TryGetValue(member.Id, out var existing)
                    || RolePriority(role) > RolePriority(existing))
                {
                    roleByUserId[member.Id] = role;
                }
            }
        }

        return users
            .Select(u => new KeycloakUserRecord(
                u.Id,
                u.Username,
                u.Email,
                u.LastName,
                u.FirstName,
                GetAttribute(u, "middleName") ?? u.MiddleName,
                roleByUserId.TryGetValue(u.Id, out var r) ? r : "user"))
            .ToList();
    }

    /// <inheritdoc />
    public async Task UpdateUserRoleAsync(string keycloakUserId, string newRole, CancellationToken ct)
    {
        var roleReps = new List<KcRoleRepresentation>();
        foreach (var role in AppRoles)
        {
            using var getRequest = await CreateRequestAsync(
                HttpMethod.Get, $"/admin/realms/{_opt.Realm}/roles/{role}", ct);
            using var getResponse = await _http.SendAsync(getRequest, ct);
            await EnsureSuccessAsync(getResponse, $"получение роли {role}", ct);
            var rep = await getResponse.Content.ReadFromJsonAsync<KcRoleRepresentation>(ct)
                ?? throw new KeycloakUnavailableException(
                    $"Роль «{role}» не найдена в realm {_opt.Realm}. " +
                    "Создайте realm-роли user, manager, admin.");
            roleReps.Add(rep);
        }

        var toRemove = roleReps
            .Where(r => r.Name != newRole)
            .Select(r => new { r.Id, r.Name })
            .ToList();
        var toAdd = roleReps
            .Where(r => r.Name == newRole)
            .Select(r => new { r.Id, r.Name })
            .ToList();

        var mappingsPath = $"/admin/realms/{_opt.Realm}/users/{keycloakUserId}/role-mappings/realm";

        if (toRemove.Count > 0)
        {
            using var removeRequest = await CreateRequestAsync(HttpMethod.Delete, mappingsPath, ct);
            removeRequest.Content = new StringContent(
                JsonSerializer.Serialize(toRemove, JsonOptions),
                Encoding.UTF8, "application/json");
            using var removeResponse = await _http.SendAsync(removeRequest, ct);
            await EnsureSuccessAsync(removeResponse, "снятие ролей", ct);
        }

        if (toAdd.Count > 0)
        {
            using var addRequest = await CreateRequestAsync(HttpMethod.Post, mappingsPath, ct);
            addRequest.Content = new StringContent(
                JsonSerializer.Serialize(toAdd, JsonOptions),
                Encoding.UTF8, "application/json");
            using var addResponse = await _http.SendAsync(addRequest, ct);
            await EnsureSuccessAsync(addResponse, "назначение роли", ct);
        }
    }

    /// <inheritdoc />
    public async Task SetUserEnabledAsync(string keycloakUserId, bool enabled, CancellationToken ct)
    {
        using var request = await CreateRequestAsync(
            HttpMethod.Put, $"/admin/realms/{_opt.Realm}/users/{keycloakUserId}", ct);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new { enabled }),
            Encoding.UTF8, "application/json");
        using var response = await _http.SendAsync(request, ct);
        await EnsureSuccessAsync(response, "блокировка/разблокировка", ct);
    }

    private async Task<string> GetAdminTokenAsync(CancellationToken ct)
    {
        await _tokenLock.WaitAsync(ct);
        try
        {
            if (_token != null && DateTime.UtcNow < _tokenExpiresAt)
            {
                return _token;
            }

            var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _opt.AdminClientId,
                ["client_secret"] = _opt.AdminClientSecret,
            });

            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsync(
                    $"/realms/{_opt.Realm}/protocol/openid-connect/token", body, ct);
            }
            catch (Exception ex)
            {
                throw new KeycloakUnavailableException(
                    "Не удалось подключиться к Keycloak.", ex);
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new KeycloakUnavailableException(
                    $"Keycloak отклонил запрос токена: {(int)response.StatusCode}. " +
                    "Проверьте client_id/client_secret сервисного аккаунта.");
            }

            var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(ct)
                ?? throw new KeycloakUnavailableException("Пустой ответ токена Keycloak.");

            _token = payload.AccessToken;
            // Запас 30 секунд до истечения
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(payload.ExpiresIn - 30);
            return _token;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task<HttpRequestMessage> CreateRequestAsync(
        HttpMethod method, string path, CancellationToken ct)
    {
        var token = await GetAdminTokenAsync(ct);
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    /// <summary>
    /// Детали ошибки (тело ответа) уходят в лог, в исключение — только
    /// краткое сообщение для фронта.
    /// </summary>
    private async Task EnsureSuccessAsync(
        HttpResponseMessage response, string operation, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var details = await response.Content.ReadAsStringAsync(ct);
        _logger.LogWarning(
            "Keycloak Admin API: {Operation} -> {Status}. Тело: {Body}",
            operation, (int)response.StatusCode, details);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound
            && operation.Contains("рол"))
        {
            throw new KeycloakUnavailableException(
                $"Keycloak: операция «{operation}» вернула 404 — у сервисного " +
                "аккаунта нет прав (нужны realm-management: view-users, " +
                "manage-users) либо роль не существует.");
        }

        throw new KeycloakUnavailableException(
            $"Keycloak отклонил операцию «{operation}» ({(int)response.StatusCode}). " +
            "Проверьте права сервисного аккаунта (realm-management).");
    }

    private static int RolePriority(string role) => role switch
    {
        "admin" => 3,
        "manager" => 2,
        _ => 1,
    };

    private static string? GetAttribute(KcUserBrief user, string name)
    {
        if (user.Attributes == null)
        {
            return null;
        }

        return user.Attributes.TryGetValue(name, out var values)
               && values is { Count: > 0 }
            ? values[0]
            : null;
    }

    // ---------- DTO Keycloak ----------

    private sealed class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = "";

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    private sealed class KcUserBrief
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("middleName")]
        public string? MiddleName { get; set; }

        [JsonPropertyName("attributes")]
        public Dictionary<string, List<string>>? Attributes { get; set; }
    }

    private sealed class KcRoleRepresentation
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
}