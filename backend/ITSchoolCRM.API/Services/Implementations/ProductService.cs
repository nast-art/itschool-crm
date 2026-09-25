using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Products;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис справочника ИТ-продуктов.
///
/// КЭШИРОВАНИЕ (KeyDB, cache-aside):
///   - GetAllAsync / GetActiveAsync → по одному общему ключу
///     (catalog:products:v{catalog:version} и
///     catalog:active-products:v{...}). Выборки не фильтруются
///     ни по доступу, ни по другим параметрам — скоп пользователя
///     в ключе не нужен, кэш общий на все роли;
///   - GetByIdAsync → без кэша: точечный вызов, выигрыш нулевой;
///   - SearchAsync → без кэша осознанно: подстрока на каждую
///     клавишу — поток одноразовых ключей, кэш вреден.
///
/// ИНВАЛИДАЦИЯ: Create/Update/Delete после SaveChangesAsync делают
/// два INCR:
///   - catalog:version — сами справочники продуктов;
///   - interaction:version — имя и ВЕНДОР продукта проецируются
///     в InteractionDto (ProductName) и в отчёты (колонки
///     «ИТ-продукт» и «Вендор» — ReportRowDto.ProductName/
///     VendorName), агрегируются в ByProduct статистики.
///     Пропуск interaction:version здесь дал бы рассинхрон
///     «на экране одно, в отчёте другое».
/// </summary>
public class ProductService : IProductService
{
    private readonly CrmDbContext _context;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    public ProductService(
        CrmDbContext context,
        ICacheService cache,
        CacheOptions cacheOptions)
    {
        _context = context;
        _cache = cache;
        _cacheOptions = cacheOptions;
    }

    /// <summary>
    /// Сброс кэша после любой записи в продукты. Два счётчика:
    /// справочники и всё, где продукт и его вендор отображаются
    /// (взаимодействия, статистика, отчёты).
    /// </summary>
    private async Task InvalidateProductsAsync(
        CancellationToken cancellationToken)
    {
        await _cache.BumpVersionAsync(
            CacheKeys.CatalogVersion,
            cancellationToken);

        await _cache.BumpVersionAsync(
            CacheKeys.InteractionVersionKey,
            cancellationToken);
    }

    public async Task<List<ProductDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        // Ключ без скопа: выборка не зависит от пользователя.
        // Промах → загрузка из PostgreSQL, результат в KeyDB
        // с TTL CatalogTtl (по умолчанию 600 сек).
        return await _cache.GetOrCreateAsync(
            CacheKeys.Catalog(CacheKeys.Products),
            _cacheOptions.CatalogTtl,
            _ => LoadAllFromDatabaseAsync(
                cancellationToken),
            cancellationToken);
    }

    /// <summary>Исходная выборка продуктов (бывшее тело GetAllAsync).</summary>
    private async Task<List<ProductDto>> LoadAllFromDatabaseAsync(
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
        // БЕЗ КЭША: точечный вызов (карточки, валидации при
        // создании/редактировании взаимодействия), обращений мало.
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

        // НОВОЕ (кэш): продукт появился в справочниках и доступен
        // для привязки к взаимодействиям. Возвращаемый DTO
        // собираем руками — он свежий по определению, лишний
        // запрос через GetByIdAsync не нужен.
        await InvalidateProductsAsync(cancellationToken);

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

        // НОВОЕ (кэш): могли измениться имя, ВЕНДОР (колонка
        // «Вендор» в таблице вузов и в отчётах строится из
        // product.vendor), активность — все списки и отчёты
        // показывают старые значения.
        await InvalidateProductsAsync(cancellationToken);

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

        // НОВОЕ (кэш): деактивация меняет общий список, список
        // активных и фильтры на фронте.
        await InvalidateProductsAsync(cancellationToken);

        return true;
    }

    public async Task<List<ProductDto>> GetActiveAsync(
        CancellationToken cancellationToken)
    {
        // Отдельный ключ от GetAllAsync: составы выборок различаются
        // (is_active-фильтр), общий ключ дал бы либо постоянные
        // промахи, либо отдачу неактивных продуктов.
        return await _cache.GetOrCreateAsync(
            CacheKeys.Catalog("catalog:active-products"),
            _cacheOptions.CatalogTtl,
            _ => LoadActiveFromDatabaseAsync(
                cancellationToken),
            cancellationToken);
    }

    /// <summary>Исходная выборка активных продуктов (бывшее тело GetActiveAsync).</summary>
    private async Task<List<ProductDto>> LoadActiveFromDatabaseAsync(
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
        // БЕЗ КЭША ОСОЗНАННО: поисковая подстрока меняется на каждую
        // клавишу — каждая её версия это новый одноразовый ключ.
        // Такие значения никто не читает повторно: кэш тут — это
        // утечка памяти KeyDB, а не ускорение.
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