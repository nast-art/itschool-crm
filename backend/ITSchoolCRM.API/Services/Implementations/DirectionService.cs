using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Directions;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис справочника ИТ-направлений.
/// </summary>
/// <remarks>
/// КЭШИРОВАНИЕ (KeyDB, cache-aside):
///   - GetAllAsync / GetActiveAsync → список под общим ключом
///     catalog:directions:v{catalog:version}. Выборки НЕ фильтруются
///     по доступу (направления видят все роли) — поэтому скоп пользователя
///     в ключе не нужен. Исключение из скопа здесь осознанное:
///     менеджер и админ видят один и тот же справочник;
///   - GetByIdAsync → без кэша: точечный вызов, выигрыш нулевой;
///   - SearchAsync → без кэша осознанно: подстрока меняется на каждую
///     клавишу, каждая буква — новый одноразовый ключ.
///
/// ИНВАЛИДАЦИЯ: Create/Update/Delete после SaveChangesAsync делают два INCR:
///   - catalog:version — сами справочники направлений;
///   - interaction:version — взаимодействия и статистика: имя направления
///     проецируется в InteractionDto через program.direction и агрегируется
///     в ReportService.ByDirection, а приоритет (ранжирование) влияет на
///     порядок выдачи.
/// </remarks>
public class DirectionService : IDirectionService
{
    private readonly CrmDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    public DirectionService(
        CrmDbContext context,
        IAuditService auditService,
        ICacheService cache,
        CacheOptions cacheOptions)
    {
        _context = context;
        _auditService = auditService;
        _cache = cache;
        _cacheOptions = cacheOptions;
    }

    /// <summary>
    /// Сброс кэша после любой записи в направления. Два счётчика:
    /// справочники и всё, где имя направления отображается
    /// или агрегируется (взаимодействия, статистика).
    /// </summary>
    private async Task InvalidateDirectionsAsync(CancellationToken cancellationToken)
    {
        await _cache.BumpVersionAsync(CacheKeys.CatalogVersion, cancellationToken);
        await _cache.BumpVersionAsync(CacheKeys.InteractionVersionKey, cancellationToken);
    }

    // Единая проекция сущности -> DTO
    private static IQueryable<DirectionDto> Project(IQueryable<it_direction> query)
    {
        return query.Select(x => new DirectionDto
        {
            Id = x.it_directions_id,
            Name = x.name,
            Description = x.description,
            IsActive = x.is_active,
            Priority = x.priority,
            CreatedAt = x.created_at,
            UpdatedAt = x.updated_at
        });
    }

    // Сортировка по умолчанию: сначала с ручным приоритетом (по возрастанию),
    // затем без него — по имени. Так фронт получает направления уже
    // в порядке ранжирования.
    private static IQueryable<it_direction> ApplyOrder(IQueryable<it_direction> query)
    {
        return query
            .OrderBy(x => x.priority == null)
            .ThenBy(x => x.priority)
            .ThenBy(x => x.name);
    }

    public async Task<List<DirectionDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        // Ключ без скопа: выборка не зависит от пользователя — общий кэш на всех.
        return await _cache.GetOrCreateAsync(
            CacheKeys.Catalog(CacheKeys.Directions),
            _cacheOptions.CatalogTtl,
            _ => Project(ApplyOrder(_context.it_directions.AsNoTracking()))
                .ToListAsync(cancellationToken),
            cancellationToken);
    }

    public async Task<DirectionDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        // БЕЗ КЭША: точечный вызов (карточки, привязки программ),
        // обращений мало, ключ с версией не дал бы выигрыша.
        return await Project(
                _context.it_directions
                    .AsNoTracking()
                    .Where(x => x.it_directions_id == id))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<DirectionDto>> GetActiveAsync(CancellationToken cancellationToken)
    {
        // Отдельный ключ от GetAllAsync: составы различаются
        // (is_active-фильтр) — общий ключ дал бы промахи.
        return await _cache.GetOrCreateAsync(
            CacheKeys.Catalog("catalog:active-directions"),
            _cacheOptions.CatalogTtl,
            _ => Project(
                    ApplyOrder(
                        _context.it_directions
                            .AsNoTracking()
                            .Where(x => x.is_active == true)))
                .ToListAsync(cancellationToken),
            cancellationToken);
    }

    public async Task<List<DirectionDto>> SearchAsync(string? search, CancellationToken cancellationToken)
    {
        // БЕЗ КЭША ОСОЗНАННО: каждая клавиша в поиске — новый ключ,
        // все они протухнут неиспользованными. Кэш одноразовых значений —
        // это утечка памяти KeyDB.
        var query = _context.it_directions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(
                x => x.name != null &&
                     x.name.ToLower().Contains(term));
        }

        return await Project(ApplyOrder(query))
            .ToListAsync(cancellationToken);
    }

    public async Task<DirectionDto?> CreateAsync(CreateDirectionDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Название направления обязательно.");
        }

        var name = dto.Name.Trim();

        var duplicateExists = await _context.it_directions
            .AnyAsync(
                x => x.name != null && x.name.ToLower() == name.ToLower(),
                cancellationToken);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "Направление с таким названием уже существует.");
        }

        var direction = new it_direction
        {
            name = name,
            description = dto.Description,
            is_active = dto.IsActive ?? true,
            priority = dto.Priority,
            created_at = DateTime.UtcNow
        };

        _context.it_directions.Add(direction);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            "CREATE",
            "it_direction",
            direction.it_directions_id,
            null,
            new
            {
                direction.it_directions_id,
                direction.name,
                direction.description,
                direction.is_active,
                direction.priority
            },
            cancellationToken);

        // Возвращаемый DTO собираем через GetByIdAsync — один запрос
        // вместо повторной проекции, в кэш он не попадёт.
        await InvalidateDirectionsAsync(cancellationToken);

        return await GetByIdAsync(direction.it_directions_id, cancellationToken);
    }

    public async Task<bool> UpdateAsync(int id, UpdateDirectionDto dto, CancellationToken cancellationToken)
    {
        var direction = await _context.it_directions
            .FirstOrDefaultAsync(x => x.it_directions_id == id, cancellationToken);

        if (direction is null)
        {
            return false;
        }

        // Если имя меняется — проверяем уникальность
        if (!string.IsNullOrWhiteSpace(dto.Name) &&
            dto.Name.Trim().ToLower() != (direction.name ?? string.Empty).ToLower())
        {
            var name = dto.Name.Trim();

            var duplicateExists = await _context.it_directions
                .AnyAsync(
                    x =>
                        x.it_directions_id != id &&
                        x.name != null &&
                        x.name.ToLower() == name.ToLower(),
                    cancellationToken);

            if (duplicateExists)
            {
                throw new InvalidOperationException(
                    "Направление с таким названием уже существует.");
            }

            direction.name = name;
        }

        var oldSnapshot = new
        {
            direction.it_directions_id,
            direction.name,
            direction.description,
            direction.is_active,
            direction.priority
        };

        if (dto.Description != null)
        {
            direction.description = dto.Description;
        }

        if (dto.IsActive.HasValue)
        {
            direction.is_active = dto.IsActive;
        }

        // Приоритет обновляем ТОЛЬКО если передан явно:
        // null от фронта означает «не менять»
        if (dto.Priority.HasValue)
        {
            direction.priority = dto.Priority.Value;
        }

        direction.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            "UPDATE",
            "it_direction",
            id,
            oldSnapshot,
            new
            {
                direction.it_directions_id,
                direction.name,
                direction.description,
                direction.is_active,
                direction.priority
            },
            cancellationToken);

        // Имя, активность или приоритет (порядок ранжирования карточек
        // на фронте) изменились — справочник и производные выборки устарели.
        await InvalidateDirectionsAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var direction = await _context.it_directions
            .FirstOrDefaultAsync(x => x.it_directions_id == id, cancellationToken);

        if (direction is null)
        {
            return false;
        }

        // Не даём удалить направление, к которому привязаны программы —
        // иначе сломаются взаимодействия
        var hasPrograms = await _context.it_programs
            .AnyAsync(x => x.direction_id == id, cancellationToken);

        if (hasPrograms)
        {
            throw new InvalidOperationException(
                "Нельзя удалить направление: к нему привязаны ИТ-программы. Деактивируйте его вместо удаления.");
        }

        var oldSnapshot = new
        {
            direction.it_directions_id,
            direction.name,
            direction.description,
            direction.is_active,
            direction.priority
        };

        _context.it_directions.Remove(direction);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            "DELETE",
            "it_direction",
            id,
            oldSnapshot,
            null,
            cancellationToken);

        await InvalidateDirectionsAsync(cancellationToken);

        return true;
    }
}