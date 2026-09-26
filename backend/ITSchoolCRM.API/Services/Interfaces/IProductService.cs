using ITSchoolCRM.API.DTOs.Products;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
    Task<List<ProductDto>> GetActiveAsync(CancellationToken cancellationToken);

    Task<List<ProductDto>> SearchAsync(string? search, CancellationToken cancellationToken);
}