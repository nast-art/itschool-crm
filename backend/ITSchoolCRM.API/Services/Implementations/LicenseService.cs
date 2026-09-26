using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Licenses;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис справочника лицензий.
/// </summary>
/// <remarks>
/// КЭШИРОВАНИЕ (KeyDB, cache-aside):
///   • GetAllAsync → один общий ключ catalog:licenses:v{catalog:version}.
///     Список не фильтруется ни по доступу, ни по другим параметрам
///     (статус передачи, сроки фильтруются на фронте из полного списка) —
///     скоп пользователя в ключе не нужен;
///   • Записей в этом сервисе нет: лицензии наполняются импортом каталога
///     (ImportService). Инвалидация живёт там — импорт уже делает
///     BumpVersionAsync(CatalogVersion), отдельная точка сброса не требуется.
///
/// ПОЧЕМУ ДВОЙНОЙ BUMP В ИМПОРТЕ ВАЖЕН ИМЕННО ЗДЕСЬ: данные лицензий
/// (подписание, срок действия, статус передачи, комментарий) отображаются
/// в таблице вузов на фронте — колонки «Подписание лицензии»,
/// «Срок действия», «Статус передачи», «Комментарий» строятся из
/// licenseById[i.licenseId]. Поэтому импорт обязан инвалидировать не только
/// catalog:version, но и interaction:version — иначе таблица вузов показала
/// бы старые сроки и статусы до истечения TTL.
/// </remarks>
public class LicenseService : ILicenseService
{
    private readonly CrmDbContext _context;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    public LicenseService(
        CrmDbContext context,
        ICacheService cache,
        CacheOptions cacheOptions)
    {
        _context = context;
        _cache = cache;
        _cacheOptions = cacheOptions;
    }

    public async Task<List<LicenseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        // Ключ без скопа: выборка не зависит от пользователя.
        // Сбрасывается при импорте каталога (ImportService делает
        // BumpVersionAsync(CatalogVersion) + BumpVersionAsync(InteractionVersionKey)).
        return await _cache.GetOrCreateAsync(
            CacheKeys.Catalog("catalog:licenses"),
            _cacheOptions.CatalogTtl,
            _ => LoadAllFromDatabaseAsync(cancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Выборка лицензий из БД (промах кэша). Сортировка по дате подписания —
    /// свежие сверху, тот же порядок, что ожидает фронт при отображении.
    /// </summary>
    private async Task<List<LicenseDto>> LoadAllFromDatabaseAsync(CancellationToken cancellationToken)
    {
        return await _context.licenses
            .AsNoTracking()
            .OrderByDescending(x => x.signed_at)
            .Select(x => new LicenseDto
            {
                Id = x.licenses_id,
                SignedAt = x.signed_at,
                ValidUntil = x.valid_until,
                TransferStatus = x.transfer_status,
                Comment = x.comment
            })
            .ToListAsync(cancellationToken);
    }
}