using ITSchoolCRM.API.DTOs.Imports;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IImportService
{
    Dictionary<string, string>
        GetAvailableMappingFields();

    Task<ImportBatchDto> ImportExcelAsync(
        IFormFile file,
        ImportMappingDto mapping,
        CancellationToken cancellationToken);

    Task<ImportBatchDto> ImportJsonAsync(
        IFormFile file,
        CancellationToken cancellationToken);

    Task<List<ImportBatchDto>> GetBatchesAsync(
        CancellationToken cancellationToken);

    Task<ImportBatchDto?> GetBatchByIdAsync(
        int id,
        CancellationToken cancellationToken);
}