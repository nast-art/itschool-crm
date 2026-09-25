using ITSchoolCRM.API.DTOs.Licenses;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface ILicenseService
{
    // Справочник лицензий — колонки «Подписание лицензии»,
    // «Срок действия», «Статус передачи», «Комментарий»
    Task<List<LicenseDto>> GetAllAsync(
        CancellationToken cancellationToken);
}