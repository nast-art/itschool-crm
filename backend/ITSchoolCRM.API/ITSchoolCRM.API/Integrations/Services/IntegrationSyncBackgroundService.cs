using ITSchoolCRM.API.Integrations.Common;
using ITSchoolCRM.API.Integrations.LMS.Services;
using ITSchoolCRM.API.Integrations.Website.Services;

namespace ITSchoolCRM.API.Integrations.Services
{
    /// <summary>
    /// Фоновая служба периодической синхронизации с внешними системами.
    /// Демонстрирует рабочий цикл двусторонней интеграции на заглушках:
    /// периодически забирает данные из LMS и с сайта, фиксирует их в логе.
    /// Интервал задаётся конфигурацией Integrations:SyncIntervalMinutes
    /// (по умолчанию — 60 минут).
    /// </summary>
    public class IntegrationSyncBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly IConfiguration _configuration;

        private readonly ILogger<IntegrationSyncBackgroundService> _logger;

        public IntegrationSyncBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            ILogger<IntegrationSyncBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            var intervalMinutes =
                _configuration.GetValue<int?>(
                    "Integrations:SyncIntervalMinutes") ?? 60;

            _logger.LogInformation(
                "Фоновая синхронизация интеграций запущена. Интервал: {IntervalMinutes} мин.",
                intervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(
                        TimeSpan.FromMinutes(intervalMinutes),
                        stoppingToken);

                    await SyncOnceAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Ошибка при фоновой синхронизации интеграций.");
                }
            }

            _logger.LogInformation(
                "Фоновая синхронизация интеграций остановлена.");
        }

        private async Task SyncOnceAsync(
            CancellationToken cancellationToken)
        {
            using var scope =
                _serviceScopeFactory.CreateScope();

            var lmsService =
                scope.ServiceProvider
                    .GetRequiredService<ILmsIntegrationService>();

            var websiteService =
                scope.ServiceProvider
                    .GetRequiredService<IWebsiteIntegrationService>();

            try
            {
                var students =
                    await lmsService.GetStudentsAsync(
                        cancellationToken);

                var enrollments =
                    await lmsService.GetEnrollmentsAsync(
                        cancellationToken);

                _logger.LogInformation(
                    "Синхронизация с LMS завершена: обучающихся — {StudentsCount}, записей на курсы — {EnrollmentsCount}.",
                    students.Count,
                    enrollments.Count);
            }
            catch (IntegrationException ex)
            {
                _logger.LogWarning(
                    "Синхронизация с LMS пропущена: {ErrorCode}. {Message}",
                    ex.ErrorCode,
                    ex.Message);
            }

            try
            {
                var applications =
                    await websiteService.GetApplicationsAsync(
                        cancellationToken);

                _logger.LogInformation(
                    "Синхронизация с веб-сайтом завершена: заявок — {ApplicationsCount}.",
                    applications.Count);
            }
            catch (IntegrationException ex)
            {
                _logger.LogWarning(
                    "Синхронизация с веб-сайтом пропущена: {ErrorCode}. {Message}",
                    ex.ErrorCode,
                    ex.Message);
            }
        }
    }
}