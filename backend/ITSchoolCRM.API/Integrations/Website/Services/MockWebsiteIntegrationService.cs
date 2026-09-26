using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ITSchoolCRM.API.Integrations.Common;
using ITSchoolCRM.API.Integrations.Website.DTOs;
using ITSchoolCRM.API.Integrations.Website.Options;
using Microsoft.Extensions.Options;

namespace ITSchoolCRM.API.Integrations.Website.Services;

/// <summary>
/// Заглушка клиента интеграции с веб-сайтом.
/// Обращается по HTTP к эндпоинтам, которые имитируют внешний сайт
/// (см. WebsiteMockController). Поведение аналогично работе с реальным
/// сайтом: JSON, заголовок API-ключа, таймауты, повторные попытки.
/// </summary>
public class MockWebsiteIntegrationService : IWebsiteIntegrationService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly WebsiteIntegrationOptions _options;
    private readonly ILogger<MockWebsiteIntegrationService> _logger;

    public MockWebsiteIntegrationService(IHttpClientFactory httpClientFactory, IOptions<WebsiteIntegrationOptions> options, ILogger<MockWebsiteIntegrationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<WebsiteApplicationDto>> GetApplicationsAsync(CancellationToken cancellationToken)
    {
        var applications = await SendAsync<List<WebsiteApplicationDto>>(HttpMethod.Get, "api/mock-website/applications",
            payload: null, cancellationToken);

        return applications ?? new List<WebsiteApplicationDto>();
    }

    public async Task PushInteractionAsync(PushInteractionToWebsiteDto dto, CancellationToken cancellationToken)
    {
        await SendAsync<object>(HttpMethod.Post, "api/mock-website/interactions",
            payload: dto, cancellationToken);
    }

    /// <summary>
    /// Универсальный метод отправки HTTP-запроса к сайту с повторными попытками.
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
                    HttpCompletionOption.ResponseHeadersRead,cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync(cancellationToken);

                    throw new IntegrationException(IntegrationErrorCodes.WebsiteBadResponse,
                        $"Веб-сайт вернул код {(int)response.StatusCode}: {body}");
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
                when (ex is HttpRequestException
                    or TaskCanceledException
                    or IOException)
            {
                if (attempt >= _options.RetryCount)
                {
                    _logger.LogError(
                        ex,
                        "Не удалось выполнить запрос к веб-сайту ({Method} {Url}) после {Attempts} попыток.",
                        method,
                        relativeUrl,
                        attempt);

                    throw new IntegrationException(
                        IntegrationErrorCodes.WebsiteUnavailable,
                        "Веб-сайт временно недоступен.",
                        ex);
                }

                _logger.LogWarning(
                    "Попытка {Attempt} обращения к веб-сайту завершилась ошибкой. Повтор через {Delay} мс.",
                    attempt,
                    _options.RetryDelayMilliseconds);

                await Task.Delay(_options.RetryDelayMilliseconds, cancellationToken);
            }
        }
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient("WebsiteIntegration");

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
                "Интеграция с веб-сайтом отключена в конфигурации (Integrations:Website:Enabled = false).");
        }

        if (string.IsNullOrWhiteSpace(_options.BaseUrl))
        {
            throw new IntegrationException(IntegrationErrorCodes.ConfigurationInvalid,
                "Не задан BaseUrl интеграции с веб-сайтом (Integrations:Website:BaseUrl).");
        }
    }
}