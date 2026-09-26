using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Universities;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис справочника вузов.
/// </summary>
/// <remarks>
/// КЭШИРОВАНИЕ (KeyDB, cache-aside):
///   - GetAllAsync / GetActiveAsync → список СО СКОПОМ текущего
///     пользователя: выборка фильтруется по university_managers,
///     у каждого менеджера свой набор. manager/admin — общий
///     скоп "full". Ключ версионирован счётчиком catalog:version;
///   - GetByIdAsync → без кэша: вызывается точечно, доступ
///     зависит от текущего пользователя, а выигрыш был бы нулевой;
///   - SearchAsync → без кэша осознанно: подстрока меняется на
///     каждую клавишу в поиске верхней панели, каждая буква —
///     новый ключ, и все эти ключи протухнут неиспользованными.
///
/// ИНВАЛИДАЦИЯ: Create/Update/Delete делают два INCR:
///   - catalog:version — все справочники (название вуза светится
///     и в списках взаимодействий, и в отчётах);
///   - interaction:version — все взаимодействия и агрегаты
///     статистики (byUniversity).
/// </remarks>
public class UniversityService : IUniversityService
{
    private readonly CrmDbContext _context;
    private readonly IUserAccessService _accessService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    public UniversityService(
        CrmDbContext context,
        IUserAccessService accessService,
        ICurrentUserService currentUserService,
        ICacheService cache,
        CacheOptions cacheOptions)
    {
        _context = context;
        _accessService = accessService;
        _currentUserService = currentUserService;
        _cache = cache;
        _cacheOptions = cacheOptions;
    }

    /// <summary>
    /// Скоп ключа кэша: "full" для manager/admin (видят все вузы —
    /// общий кэш уместен), KeycloakUserId для менеджера по вузам.
    /// KeycloakUserId берём, а не БД-id: он гарантированно есть
    /// в JWT, users_id появляется только после синхронизации.
    /// </summary>
    private string? CurrentCacheScope =>
        _accessService.HasFullAccess()
            ? CacheKeys.FullAccessScope
            : _currentUserService.KeycloakUserId;

    /// <summary>
    /// Сброс кэша после любой записи в справочник вузов.
    /// catalog:version — сами справочники; interaction:version —
    /// взаимодействия и статистика (в них отображаются названия
    /// вузов и агрегируются по ним).
    /// </summary>
    private async Task InvalidateCatalogAsync(CancellationToken cancellationToken)
    {
        await _cache.BumpVersionAsync(CacheKeys.CatalogVersion, cancellationToken);
        await _cache.BumpVersionAsync(CacheKeys.InteractionVersionKey, cancellationToken);
    }

    // Единая проекция сущности -> DTO
    private static IQueryable<UniversityDto> ProjectToDto(IQueryable<university> query)
    {
        return query.Select(x => new UniversityDto
        {
            Id = x.universities_id,
            Name = x.name,
            ShortName = x.short_name,
            IsActive = x.is_active
        });
    }

    public async Task<List<UniversityDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var scope = CurrentCacheScope;

        // Пользователь не аутентифицирован (контроллеры под
        // [Authorize], но защищаемся): выборка будет пустой,
        // кэшировать нечего — идём в БД напрямую.
        if (scope is null)
        {
            return await LoadAllFromDatabaseAsync(cancellationToken);
        }

        return await _cache.GetOrCreateAsync(
            CacheKeys.CatalogForScope(CacheKeys.Universities, scope),
            _cacheOptions.CatalogTtl,
            _ => LoadAllFromDatabaseAsync(cancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Выборка списка из БД (промах кэша). Фильтрация по доступу
    /// внутри — кэш хранит УЖЕ отфильтрованный по university_managers набор.
    /// </summary>
    private async Task<List<UniversityDto>> LoadAllFromDatabaseAsync(CancellationToken cancellationToken)
    {
        var accessibleUniversityIds = _accessService.GetAccessibleUniversityIds();

        return await ProjectToDto(
                _context.universities
                    .AsNoTracking()
                    .Where(x => accessibleUniversityIds.Contains(x.universities_id)))
            .ToListAsync(cancellationToken);
    }

    public async Task<UniversityDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        // БЕЗ КЭША: метод вызывается точечно (карточки, привязки),
        // а ключ пришлось бы строить с учётом доступа текущего
        // пользователя — выигрыш нулевой, сложность лишняя.
        var accessible = await _accessService
            .HasAccessToUniversityAsync(id, cancellationToken);

        if (!accessible)
        {
            return null;
        }

        return await ProjectToDto(
                _context.universities
                    .AsNoTracking()
                    .Where(x => x.universities_id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<UniversityDto>> GetActiveAsync(CancellationToken cancellationToken)
    {
        var scope = CurrentCacheScope;

        if (scope is null)
        {
            return await LoadActiveFromDatabaseAsync(cancellationToken);
        }

        // Отдельный ключ от GetAllAsync: составы выборок различаются
        // (is_active-фильтр), общий ключ дал бы промахи.
        return await _cache.GetOrCreateAsync(
            CacheKeys.CatalogForScope("catalog:active-universities", scope),
            _cacheOptions.CatalogTtl,
            _ => LoadActiveFromDatabaseAsync(cancellationToken),
            cancellationToken);
    }

    /// <summary>Выборка активных вузов из БД (промах кэша).</summary>
    private async Task<List<UniversityDto>> LoadActiveFromDatabaseAsync(CancellationToken cancellationToken)
    {
        var accessibleUniversityIds = _accessService.GetAccessibleUniversityIds();

        return await ProjectToDto(
                _context.universities
                    .AsNoTracking()
                    .Where(x =>
                        x.is_active == true &&
                        accessibleUniversityIds.Contains(x.universities_id)))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UniversityDto>> SearchAsync(string? search, CancellationToken cancellationToken)
    {
        // БЕЗ КЭША ОСОЗНАННО: поиск из верхней панели (Topbar)
        // выполняется на каждую клавишу. Подстрока "м", "мг", "мгу" —
        // три разных ключа, все протухнут через TTL неиспользованными.
        // Кэш тут вреден: чистили бы память KeyDB под одноразовые значения.
        var accessibleUniversityIds = _accessService.GetAccessibleUniversityIds();

        var query = _context.universities
            .AsNoTracking()
            .Where(x => accessibleUniversityIds.Contains(x.universities_id))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var lowerSearch = search.Trim().ToLower();

            query = query.Where(x =>
                (x.name != null && x.name.ToLower().Contains(lowerSearch)) ||
                (x.short_name != null && x.short_name.ToLower().Contains(lowerSearch)));
        }

        return await ProjectToDto(query)
            .ToListAsync(cancellationToken);
    }

    public async Task<UniversityDto> CreateAsync(CreateUniversityDto dto, CancellationToken cancellationToken)
    {
        var university = new university
        {
            name = dto.Name,
            short_name = dto.ShortName,
            is_active = true,
            created_at = DateTime.UtcNow
        };

        _context.universities.Add(university);

        await _context.SaveChangesAsync(cancellationToken);

        // Вуз появился в справочниках, списках взаимодействий и агрегатах
        // статистики. DTO собираем руками (а не через GetByIdAsync, чтобы
        // не делать лишний запрос) — он свежий по определению.
        await InvalidateCatalogAsync(cancellationToken);

        return new UniversityDto
        {
            Id = university.universities_id,
            Name = university.name,
            ShortName = university.short_name,
            IsActive = university.is_active
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateUniversityDto dto, CancellationToken cancellationToken)
    {
        var university = await _context.universities
            .FirstOrDefaultAsync(x => x.universities_id == id, cancellationToken);

        if (university is null)
        {
            return false;
        }

        university.name = dto.Name;
        university.short_name = dto.ShortName;
        university.is_active = dto.IsActive;
        university.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Изменилось название или активность — справочники, списки
        // взаимодействий и статистика показывали бы старые значения.
        await InvalidateCatalogAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var university = await _context.universities
            .FirstOrDefaultAsync(x => x.universities_id == id, cancellationToken);

        if (university is null)
        {
            return false;
        }

        // Мягкое удаление: строка остаётся (аудит, история), снимаем активность
        university.is_active = false;
        university.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Деактивация меняет и GetActiveAsync, и все списки, где вуз отображался
        await InvalidateCatalogAsync(cancellationToken);

        return true;
    }
}