using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Products;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class ProductService : IProductService
{
    private readonly CrmDbContext _context;

    public ProductService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.it_products
            .AsNoTracking()
            .Select(x => new ProductDto
            {
                Id = x.it_products_id,
                Name = x.name,
                Vendor = x.vendor,
                Description = x.description,
                IsActive = x.is_active
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.it_products
            .AsNoTracking()
            .Where(x => x.it_products_id == id)
            .Select(x => new ProductDto
            {
                Id = x.it_products_id,
                Name = x.name,
                Vendor = x.vendor,
                Description = x.description,
                IsActive = x.is_active
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProductDto> CreateAsync(
        CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        var product = new it_product
        {
            name = dto.Name,
            vendor = dto.Vendor,
            description = dto.Description,
            is_active = true,
            created_at = DateTime.UtcNow
        };

        _context.it_products.Add(product);

        await _context.SaveChangesAsync(cancellationToken);

        return new ProductDto
        {
            Id = product.it_products_id,
            Name = product.name,
            Vendor = product.vendor,
            Description = product.description,
            IsActive = product.is_active
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateProductDto dto,
        CancellationToken cancellationToken)
    {
        var product = await _context.it_products
            .FirstOrDefaultAsync(
                x => x.it_products_id == id,
                cancellationToken);

        if (product is null)
        {
            return false;
        }

        product.name = dto.Name;
        product.vendor = dto.Vendor;
        product.description = dto.Description;
        product.is_active = dto.IsActive;
        product.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _context.it_products
            .FirstOrDefaultAsync(
                x => x.it_products_id == id,
                cancellationToken);

        if (product is null)
        {
            return false;
        }

        product.is_active = false;
        product.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<List<ProductDto>> GetActiveAsync(
    CancellationToken cancellationToken)
{
    return await _context.it_products
        .AsNoTracking()
        .Where(x => x.is_active == true)
        .Select(x => new ProductDto
        {
            Id = x.it_products_id,
            Name = x.name,
            Vendor = x.vendor,
            Description = x.description,
            IsActive = x.is_active
        })
        .ToListAsync(cancellationToken);
}

public async Task<List<ProductDto>> SearchAsync(
    string? search,
    CancellationToken cancellationToken)
{
    var query = _context.it_products
        .AsNoTracking()
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        search = search.Trim();

        query = query.Where(x =>
            (x.name != null &&
             x.name.ToLower().Contains(search.ToLower())) ||
            (x.vendor != null &&
             x.vendor.ToLower().Contains(search.ToLower())));
    }

    return await query
        .Select(x => new ProductDto
        {
            Id = x.it_products_id,
            Name = x.name,
            Vendor = x.vendor,
            Description = x.description,
            IsActive = x.is_active
        })
        .ToListAsync(cancellationToken);
}
}