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
        // Название — обязательное поле (NOT NULL + unique в БД).
        // Фронт валидирует, но защищаемся и здесь: иначе получим
        // PostgresException, завёрнутый мидлварью в 500.
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new InvalidOperationException("Название вуза обязательно.");
        }

        var now = DateTime.UtcNow;

        var university = new university
        {
            name = dto.Name.Trim(),
            short_name = dto.ShortName,
            is_active = true,
            created_at = now
        };

        _context.universities.Add(university);

        // Сначала сохраняем вуз — без этого у него нет id,
        // и любые ссылки на него (контакт, взаимодействие) нарушили бы FK.
        await _context.SaveChangesAsync(cancellationToken);

        // ---------- Договор ----------
        // contracts — самостоятельная таблица: связь с взаимодействием
        // обратная (interactions.contract_id). Поэтому договор создаём
        // и сохраняем ДО взаимодействия, иначе некуда будет ссылаться.
        contract? createdContract = null;
        if (!string.IsNullOrWhiteSpace(dto.ContractNumber))
        {
            createdContract = new contract
            {
                contract_number = dto.ContractNumber.Trim(),
                signed_at = dto.LicenseSignedAt,
                comment = dto.Comment,
                created_at = now
            };
            _context.contracts.Add(createdContract);
        }

        // ---------- Лицензия ----------
        // Аналогично договору: interactions.license_id -> licenses.
        // Создаём, если заполнена дата подписания ИЛИ срок действия.
        // Год (2027) разворачиваем в дату: срок — до конца года.
        license? createdLicense = null;
        if (dto.LicenseSignedAt is not null || dto.LicenseValidYears is not null)
        {
            createdLicense = new license
            {
                signed_at = dto.LicenseSignedAt,
                valid_until = dto.LicenseValidYears is not null
                    ? new DateTime(dto.LicenseValidYears.Value, 12, 31, 0, 0, 0, DateTimeKind.Utc)
                    : null,
                transfer_status = dto.TransferStatus,
                comment = dto.Comment,
                created_at = now
            };
            _context.licenses.Add(createdLicense);
        }

        // ---------- Контактное лицо вуза ----------
        // interactions ссылается на university_contacts (university_contact_id),
        // а не хранит ФИО строкой. Контакт привязан к вузу.
        university_contact? createdContact = null;
        if (!string.IsNullOrWhiteSpace(dto.UniversityContactName))
        {
            createdContact = new university_contact
            {
                university_id = university.universities_id,
                full_name = dto.UniversityContactName.Trim(),
                is_active = true
            };
            _context.university_contacts.Add(createdContact);
        }

        // Один пакетный SaveChanges для договора + лицензии + контакта —
        // после него у всех появятся id, которые нужны взаимодействию.
        if (createdContract is not null || createdLicense is not null || createdContact is not null)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        // ---------- Взаимодействие ----------
        // Таблица «Вузы» строится из interactions (строка = взаимодействие
        // + договор + лицензия + продукт). Без взаимодействия вуз был бы
        // в БД, но не отображался бы в реестре — создаём его сразу.
        var workflow = await _context.workflows
            .Where(w => w.is_active == true)
            .OrderBy(w => w.workflows_id)
            .FirstOrDefaultAsync(cancellationToken);

        if (workflow is not null)
        {
            var initialStatus = await _context.workflow_statuses
                .Where(s =>
                    s.workflow_id == workflow.workflows_id &&
                    s.is_initial == true)
                .OrderBy(s => s.sort_order)
                .FirstOrDefaultAsync(cancellationToken);

            var interaction = new interaction
            {
                university_id = university.universities_id,
                program_id = null,
                product_id = dto.ProductId,
                manager_id = dto.ManagerId,
                university_contact_id = createdContact?.university_contacts_id,
                workflow_id = workflow.workflows_id,
                current_status_id = initialStatus?.workflow_statuses_id,
                contract_id = createdContract?.contracts_id,
                license_id = createdLicense?.licenses_id,
                created_at = now,
                updated_at = now
            };

            _context.interactions.Add(interaction);

            await _context.SaveChangesAsync(cancellationToken);
        }

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