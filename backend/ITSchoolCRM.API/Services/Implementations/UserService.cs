using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Users;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис справочника пользователей.
/// </summary>
/// <remarks>
/// КЭШИРОВАНИЕ (KeyDB, cache-aside):
///   - GetAllAsync → один общий ключ catalog:users:v{catalog:version}.
///     Список не фильтруется по доступу (фильтр «Ответственный»
///     и назначение менеджеров доступны всем ролям) — скоп
///     пользователя в ключе не нужен;
///   - Записей в этом сервисе нет: users наполняется только
///     синхронизацией из Keycloak (UserSyncService). Инвалидация
///     живёт там.
///
/// ПОЧЕМУ НЕ КЭШИРОВАТЬ ФИО ТОЧЕЧНО: ФИО пользователя проецируется
/// в InteractionDto.ManagerName и ReportRowDto.ResponsibleName.
/// Если бы мы кэшировали по одному пользователю, пришлось бы
/// искать «в каких взаимодействиях он менеджер» — проще и
/// дешевле один INCR interaction:version, который сбрасывает
/// и списки взаимодействий, и статистику целиком.
/// </remarks>
public class UserService : IUserService
{
    private readonly CrmDbContext _context;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    public UserService(CrmDbContext context, ICacheService cache, CacheOptions cacheOptions)
    {
        _context = context;
        _cache = cache;
        _cacheOptions = cacheOptions;
    }

    public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        // Ключ без скопа: список одинаков для всех ролей.
        // Инвалидация — INCR catalog:version в UserSyncService.UpsertAsync.
        return await _cache.GetOrCreateAsync(CacheKeys.Catalog(CacheKeys.Users),
            _cacheOptions.CatalogTtl,
            _ => LoadAllFromDatabaseAsync(cancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Выборка пользователей из БД (промах кэша). Только активные —
    /// неактивные не должны попадать в фильтры «Ответственный»
    /// и в назначение менеджеров: деактивация немедленно лишает доступа.
    /// </summary>
    private async Task<List<UserDto>> LoadAllFromDatabaseAsync(CancellationToken cancellationToken)
    {
        return await _context.users
            .AsNoTracking()
            .Where(x => x.is_active == true)
            .OrderBy(x => x.last_name)
            .Select(x => new UserDto
            {
                Id = x.users_id,
                KeycloakUserId = x.keycloak_user_id,
                LastName = x.last_name,
                FirstName = x.first_name,
                MiddleName = x.middle_name,
                Email = x.email,
                IsActive = x.is_active
            })
            .ToListAsync(cancellationToken);
    }
}