using System.Collections.Concurrent;
using ITSchoolCRM.API.Integrations.Common;
using ITSchoolCRM.API.Integrations.Website.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Integrations.Website.Controllers
{
    /// <summary>
    /// Заглушка внешнего веб-сайта (CMS Laravel).
    /// Имитирует эндпоинты реального сайта:
    ///  — GET  выдаёт заявки, оставленные на сайте;
    ///  — POST принимает данные взаимодействия из CRM (направление CRM → сайт).
    /// Доступ защищён заголовком X-Api-Key, проверяемым по конфигурации.
    /// Когда сайт заказчика будет доработан, этот контроллер удаляется,
    /// а MockWebsiteIntegrationService заменяется на реальный клиент.
    /// </summary>
    [ApiController]
    [Route("api/mock-website")]
    public class WebsiteMockController : ControllerBase
    {
        private static readonly ConcurrentDictionary<int, WebsiteApplicationDto> Applications =
            new()
            {
                [1] = new WebsiteApplicationDto
                {
                    Id = 1,
                    FullName = "Иванова Мария Александровна",
                    Email = "m.ivanova@example.ru",
                    Phone = "+79001234567",
                    OrganizationName = null,
                    ProgramName = "Тестировщик ПО (QA)",
                    Status = "New",
                    CreatedAt = new DateTime(2026, 9, 10, 8, 30, 0, DateTimeKind.Utc)
                },
                [2] = new WebsiteApplicationDto
                {
                    Id = 2,
                    FullName = "Смирнов Алексей Павлович",
                    Email = "a.smirnov@example.ru",
                    Phone = "+79007654321",
                    OrganizationName = "ООО «Вектор»",
                    ProgramName = "DevOps-инженер",
                    Status = "InProgress",
                    CreatedAt = new DateTime(2026, 9, 12, 11, 15, 0, DateTimeKind.Utc)
                }
            };

        private static readonly List<PushInteractionToWebsiteDto> ReceivedInteractions =
            new();

        private static readonly object ReceivedInteractionsLock =
            new();

        private readonly IConfiguration _configuration;

        private readonly ILogger<WebsiteMockController> _logger;

        public WebsiteMockController(
            IConfiguration configuration,
            ILogger<WebsiteMockController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("applications")]
        public ActionResult<List<WebsiteApplicationDto>> GetApplications()
        {
            if (!IsAuthorized())
            {
                return Unauthorized(new
                {
                    code = IntegrationErrorCodes.ConfigurationInvalid,
                    message = "Некорректный или отсутствующий API-ключ."
                });
            }

            return Ok(Applications.Values.ToList());
        }

        /// <summary>
        /// Приём данных взаимодействия из CRM (направление CRM → сайт).
        /// </summary>
        [HttpPost("interactions")]
        public ActionResult ReceiveInteraction(
            [FromBody] PushInteractionToWebsiteDto dto)
        {
            if (!IsAuthorized())
            {
                return Unauthorized(new
                {
                    code = IntegrationErrorCodes.ConfigurationInvalid,
                    message = "Некорректный или отсутствующий API-ключ."
                });
            }

            if (dto is null || dto.InteractionId <= 0)
            {
                return BadRequest(new
                {
                    code = IntegrationErrorCodes.WebsiteBadResponse,
                    message = "Тело запроса не содержит корректного идентификатора взаимодействия."
                });
            }

            lock (ReceivedInteractionsLock)
            {
                ReceivedInteractions.Add(dto);
            }

            _logger.LogInformation(
                "Mock Website принял взаимодействие {InteractionId} (ВУЗ: {UniversityName}, статус: {StatusName}).",
                dto.InteractionId,
                dto.UniversityName,
                dto.StatusName);

            return Ok(new
            {
                received = true,
                interactionId = dto.InteractionId,
                receivedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Служебный эндпоинт для проверки двустороннего обмена:
        /// показывает, какие взаимодействия сайт получил из CRM.
        /// </summary>
        [HttpGet("received-interactions")]
        public ActionResult<List<PushInteractionToWebsiteDto>> GetReceivedInteractions()
        {
            if (!IsAuthorized())
            {
                return Unauthorized(new
                {
                    code = IntegrationErrorCodes.ConfigurationInvalid,
                    message = "Некорректный или отсутствующий API-ключ."
                });
            }

            lock (ReceivedInteractionsLock)
            {
                return Ok(ReceivedInteractions.ToList());
            }
        }

        private bool IsAuthorized()
        {
            var expectedKey =
                _configuration["Integrations:MockApiKey"];

            if (string.IsNullOrWhiteSpace(expectedKey))
            {
                return true;
            }

            var providedKey =
                Request.Headers["X-Api-Key"].ToString();

            return string.Equals(
                providedKey,
                expectedKey,
                StringComparison.Ordinal);
        }
    }
}