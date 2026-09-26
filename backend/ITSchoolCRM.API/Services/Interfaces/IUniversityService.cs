using ITSchoolCRM.API.DTOs.Universities;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IUniversityService
{
    Task<List<UniversityDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<UniversityDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<List<UniversityDto>> GetActiveAsync(CancellationToken cancellationToken);

    Task<List<UniversityDto>> SearchAsync(string? search, CancellationToken cancellationToken);

    Task<UniversityDto> CreateAsync(CreateUniversityDto dto, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(int id, UpdateUniversityDto dto, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}