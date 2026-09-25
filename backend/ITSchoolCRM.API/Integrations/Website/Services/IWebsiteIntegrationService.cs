using ITSchoolCRM.API.Integrations.Website.DTOs;

namespace ITSchoolCRM.API.Integrations.Website.Services
{
    /// <summary>
    /// Клиент двусторонней интеграции с веб-сайтом.
    /// Реализация на заглушках: MockWebsiteIntegrationService.
    /// </summary>
    public interface IWebsiteIntegrationService
    {
        Task<List<WebsiteApplicationDto>> GetApplicationsAsync(
            CancellationToken cancellationToken);

        Task PushInteractionAsync(
            PushInteractionToWebsiteDto dto,
            CancellationToken cancellationToken);
    }
}