using ITSchoolCRM.API.DTOs.Programs;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IProgramService
{
    Task<List<ProgramDto>> GetAllAsync(
        int? directionId,
        CancellationToken cancellationToken);

    Task<ProgramDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<List<ProgramDto>> GetActiveAsync(
        int? directionId,
        CancellationToken cancellationToken);

    Task<List<ProgramDto>> SearchAsync(
        string? search,
        int? directionId,
        CancellationToken cancellationToken);

    Task<ProgramDto> CreateAsync(
        CreateProgramDto dto,
        CancellationToken cancellationToken);

    Task<bool> UpdateAsync(
        int id,
        UpdateProgramDto dto,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken);
}