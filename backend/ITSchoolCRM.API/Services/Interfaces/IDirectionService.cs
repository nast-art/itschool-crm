using ITSchoolCRM.API.DTOs.Directions;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IDirectionService
{
    Task<List<DirectionDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<DirectionDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<List<DirectionDto>> GetActiveAsync(CancellationToken cancellationToken);

    Task<List<DirectionDto>> SearchAsync(string? search, CancellationToken cancellationToken);

    Task<DirectionDto> CreateAsync(CreateDirectionDto dto, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(int id, UpdateDirectionDto dto, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}