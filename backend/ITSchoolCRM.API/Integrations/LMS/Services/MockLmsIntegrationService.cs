using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ITSchoolCRM.API.Integrations.Common;
using ITSchoolCRM.API.Integrations.LMS.DTOs;
using ITSchoolCRM.API.Integrations.LMS.Options;
using Microsoft.Extensions.Options;

namespace ITSchoolCRM.API.Integrations.LMS.Services;

/// <summary>
/// Заглушка клиента интеграции с LMS.
/// Обращается по HTTP к эндпоинтам, которые имитируют внешнюю LMS
/// (см. LmsMockController). Поведение полностью аналогично работе
/// с реальной LMS: сериализация JSON, заголовок API-ключа, таймауты,
/// повторные попытки при сетевых сбоях.
/// </summary>
public class MockLmsIntegrationService : ILmsIntegrationService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LmsIntegrationOptions _options;
    private readonly ILogger<MockLmsIntegrationService> _logger;

    public MockLmsIntegrationService(IHttpClientFactory httpClientFactory, IOptions<LmsIntegrationOptions> options, ILogger<MockLmsIntegrationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<LmsStudentDto>> GetStudentsAsync(CancellationToken cancellationToken)
    {
        var students = await SendAsync<List<LmsStudentDto>>(HttpMethod.Get, "api/mock-lms/students", payload: null, cancellationToken);

        return students ?? new List<LmsStudentDto>();
    }

    public async Task<List<LmsCourseDto>> GetCoursesAsync(CancellationToken cancellationToken)
    {
        var courses = await SendAsync<List<LmsCourseDto>>(HttpMethod.Get, "api/mock-lms/courses", payload: null, cancellationToken);

        return courses ?? new List<LmsCourseDto>();
    }

    public async Task<List<LmsEnrollmentDto>> GetEnrollmentsAsync(CancellationToken cancellationToken)
    {
        var enrollments = await SendAsync<List<LmsEnrollmentDto>>(HttpMethod.Get, "api/mock-lms/enrollments", payload: null, cancellationToken);

        return enrollments ?? new List<LmsEnrollmentDto>();
    }

    public async Task PushInteractionAsync(PushInteractionToLmsDto dto, CancellationToken cancellationToken)
    {
        await SendAsync<object>(HttpMethod.Post, "api/mock-lms/interactions", payload: dto, cancellationToken);
    }

    /// <summary>
    /// Универсальный метод отправки HTTP-запроса к LMS с повторными попытками.
    /// </summary>
    private async Task<T?> SendAsync<T>(HttpMethod method, string relativeUrl, object? payload, CancellationToken cancellationToken)
    {
        EnsureEnabled();

        var attempt = 0;

        while (true)
        {
            attempt++;

            try
            {
                using var client = CreateClient();

                using var request = new HttpRequestMessage(method, relativeUrl);

                if (payload is not null)
                {
                    var json = JsonSerializer.Serialize(payload);

                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                using var response = await client.SendAsync(request,
                    HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);

                    throw new IntegrationException(
                        IntegrationErrorCodes.LmsBadResponse,
                        $"LMS вернула код {(int)response.StatusCode}: {body}");
                }

                // Push-операции не читают тело ответа
                if (typeof(T) == typeof(object))
                {
                    return default;
                }

                var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);

                return JsonSerializer.Deserialize<T>(responseJson, JsonOptions);
            }
            catch (IntegrationException)
            {
                // Осмысленная ошибка интеграции — не ретраим,
                // пробрасываем как есть.
                throw;
            }
            catch (Exception ex)
                when (ex is HttpRequestException or TaskCanceledException or IOException)
            {
                if (attempt >= _options.RetryCount)
                {
                    _logger.LogError(ex,
                        "Не удалось выполнить запрос к LMS ({Method} {Url}) после {Attempts} попыток.",
                        method,relativeUrl,attempt);

                    throw new IntegrationException(IntegrationErrorCodes.LmsUnavailable,
                        "Внешняя LMS временно недоступна.",ex);
                }

                _logger.LogWarning("Попытка {Attempt} обращения к LMS завершилась ошибкой. Повтор через {Delay} мс.",
                    attempt,_options.RetryDelayMilliseconds);

                await Task.Delay(_options.RetryDelayMilliseconds, cancellationToken);
            }
        }
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("LmsIntegration");

        client.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            client.DefaultRequestHeaders.Remove("X-Api-Key");
            client.DefaultRequestHeaders.Add("X-Api-Key", _options.ApiKey);
        }

        return client;
    }

    private void EnsureEnabled()
    {
        if (!_options.Enabled)
        {
            throw new IntegrationException(IntegrationErrorCodes.ConfigurationInvalid,
                "Интеграция с LMS отключена в конфигурации (Integrations:LMS:Enabled = false).");
        }

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new IntegrationException(IntegrationErrorCodes.ConfigurationInvalid,
                "Не задан BaseUrl интеграции с LMS (Integrations:LMS:BaseUrl).");
        }
    }
}