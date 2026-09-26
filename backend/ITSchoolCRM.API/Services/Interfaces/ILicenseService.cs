using ITSchoolCRM.API.DTOs.Licenses;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface ILicenseService
{
    Task<List<LicenseDto>> GetAllAsync(CancellationToken cancellationToken);
}