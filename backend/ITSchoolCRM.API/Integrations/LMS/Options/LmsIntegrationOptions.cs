using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Integrations.LMS.Options
{
    public class LmsIntegrationOptions
    {
        public const string SectionName = "Integrations:LMS";

        /// <summary>
        /// Базовый URL внешней LMS. Для заглушки указывается адрес этого же приложения.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// API-ключ, который CRM передаёт во внешнюю LMS в заголовке X-Api-Key.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Таймаут одного HTTP-запроса в секундах.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 15;

        /// <summary>
        /// Количество повторных попыток при сетевых сбоях.
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Пауза между повторными попытками в миллисекундах.
        /// </summary>
        public int RetryDelayMilliseconds { get; set; } = 500;

        /// <summary>
        /// Признак включения интеграции. Если false — методы выбрасывают IntegrationException.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}