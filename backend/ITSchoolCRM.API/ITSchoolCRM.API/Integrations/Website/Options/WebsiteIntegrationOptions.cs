namespace ITSchoolCRM.API.Integrations.Website.Options
{
    /// <summary>
    /// Настройки интеграции с веб-сайтом (CMS Laravel).
    /// Заполняются из секции Configuration "Integrations:Website".
    /// </summary>
    public class WebsiteIntegrationOptions
    {
        public const string SectionName = "Integrations:Website";

        /// <summary>
        /// Базовый URL веб-сайта. Для заглушки указывается адрес этого же приложения.
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// API-ключ, который CRM передаёт на сайт в заголовке X-Api-Key.
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
        /// Признак включения интеграции.
        /// </summary>
        public bool Enabled { get; set; } = true;
    }
}