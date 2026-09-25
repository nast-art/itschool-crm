using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Programs;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис справочника ИТ-программ.
///
/// КЭШИРОВАНИЕ (KeyDB, cache-aside):
///   - GetAllAsync / GetActiveAsync → по ДВА ключа на метод:
///     общий (все программы) и per-direction (фильтр directionId).
///     Оба без скопа пользователя — выборки не фильтруются
///     по доступу. Отдельные ключи обязательны: общий ключ под
///     фильтрованной выборкой дал бы либо промахи, либо
///     неверные данные;
///   - GetByIdAsync → без кэша: точечный вызов, выигрыш нулевой;
///   - SearchAsync → без кэша осознанно: подстрока на каждую
///     клавишу — это поток одноразовых ключей.
///
/// ИНВАЛИДАЦИЯ: Create/Update/Delete после SaveChangesAsync делают
/// два INCR:
///   - catalog:version — сами справочники программ;
///   - interaction:version — имя программы проецируется в
///     InteractionDto.ProgramName и отображается в списках,
///     карточках и отчётах. При смене direction_id у программы
///     меняется и агрегат ByDirection статистики — тот же счётчик.
/// </summary>
public class ProgramService : IProgramService
{
    private readonly CrmDbContext _context;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    public ProgramService(
        CrmDbContext context,
        ICacheService cache,
        CacheOptions cacheOptions)
    {
        _context = context;
        _cache = cache;
        _cacheOptions = cacheOptions;
    }

    /// <summary>
    /// Сброс кэша после любой записи в программы. Два счётчика:
    /// справочники и всё, где имя программы отображается
    /// (взаимодействия, статистика, отчёты).
    /// </summary>
    private async Task InvalidateProgramsAsync(
        CancellationToken cancellationToken)
    {
        await _cache.BumpVersionAsync(
            CacheKeys.CatalogVersion,
            cancellationToken);

        await _cache.BumpVersionAsync(
            CacheKeys.InteractionVersionKey,
            cancellationToken);
    }

    public async Task<List<ProgramDto>> GetAllAsync(
        int? directionId,
        CancellationToken cancellationToken)
    {
        // Фильтр задан → отдельный per-direction ключ. Общий ключ
        // под выборкой «только направление N» либо промахнулся бы
        // на каждый запрос, либо (при записи под общий ключ)
        // отдавал бы отфильтрованные данные запросам без фильтра.
        if (directionId.HasValue)
        {
            return await _cache.GetOrCreateAsync(
                CacheKeys.Catalog(
                    $"catalog:programs:direction:{directionId.Value}"),
                _cacheOptions.CatalogTtl,
                _ => LoadAllFromDatabaseAsync(
                    directionId,
                    cancellationToken),
                cancellationToken);
        }

        return await _cache.GetOrCreateAsync(
            CacheKeys.Catalog(CacheKeys.Programs),
            _cacheOptions.CatalogTtl,
            _ => LoadAllFromDatabaseAsync(
                null,
                cancellationToken),
            cancellationToken);
    }

    /// <summary>Исходная выборка программ (бывшее тело GetAllAsync).</summary>
    private async Task<List<ProgramDto>> LoadAllFromDatabaseAsync(
        int? directionId,
        CancellationToken cancellationToken)
    {
        var query = _context.it_programs
            .AsNoTracking()
            .AsQueryable();

        if (directionId.HasValue)
        {
            query = query.Where(
                x => x.direction_id == directionId.Value);
        }

        return await query
            .Select(x => new ProgramDto
            {
                Id = x.it_programs_id,
                DirectionId = x.direction_id,
                Name = x.name,
                Description = x.description,
                IsActive = x.is_active
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProgramDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        // БЕЗ КЭША: точечный вызов (карточки, привязки,
        // валидации при создании взаимодействия), обращений мало.
        return await _context.it_programs
            .AsNoTracking()
            .Where(x => x.it_programs_id == id)
            .Select(x => new ProgramDto
            {
                Id = x.it_programs_id,
                DirectionId = x.direction_id,
                Name = x.name,
                Description = x.description,
                IsActive = x.is_active
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProgramDto> CreateAsync(
        CreateProgramDto dto,
        CancellationToken cancellationToken)
    {
        var program = new it_program
        {
            direction_id = dto.DirectionId,
            name = dto.Name,
            description = dto.Description,
            is_active = true,
            created_at = DateTime.UtcNow
        };

        _context.it_programs.Add(program);

        await _context.SaveChangesAsync(cancellationToken);

        // НОВОЕ (кэш): программа появилась в общем списке, списке
        // своего направления и в выборках взаимодействий. Возвращаемый
        // DTO собираем руками — он свежий по определению, лишний
        // запрос через GetByIdAsync не нужен.
        await InvalidateProgramsAsync(cancellationToken);

        return new ProgramDto
        {
            Id = program.it_programs_id,
            DirectionId = program.direction_id,
            Name = program.name,
            Description = program.description,
            IsActive = program.is_active
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateProgramDto dto,
        CancellationToken cancellationToken)
    {
        var program = await _context.it_programs
            .FirstOrDefaultAsync(
                x => x.it_programs_id == id,
                cancellationToken);

        if (program is null)
        {
            return false;
        }

        program.direction_id = dto.DirectionId;
        program.name = dto.Name;
        program.description = dto.Description;
        program.is_active = dto.IsActive;
        program.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // НОВОЕ (кэш): могли измениться имя (списки, карточки,
        // отчёты), активность (GetActiveAsync и фильтры фронта)
        // и direction_id (перепривязка к другому направлению —
        // меняется и группировка в статистике ByDirection).
        await InvalidateProgramsAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var program = await _context.it_programs
            .FirstOrDefaultAsync(
                x => x.it_programs_id == id,
                cancellationToken);

        if (program is null)
        {
            return false;
        }

        program.is_active = false;
        program.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // НОВОЕ (кэш): деактивация меняет общий список, список
        // активных и списки взаимодействий (программа перестала
        // быть доступной для новых привязок).
        await InvalidateProgramsAsync(cancellationToken);

        return true;
    }

    public async Task<List<ProgramDto>> GetActiveAsync(
        int? directionId,
        CancellationToken cancellationToken)
    {
        // Два ключа, как в GetAllAsync: общий и per-direction.
        // Составы выборок различаются (is_active-фильтр),
        // ключи от GetAllAsync тоже разные.
        if (directionId.HasValue)
        {
            return await _cache.GetOrCreateAsync(
                CacheKeys.Catalog(
                    $"catalog:active-programs:direction:{directionId.Value}"),
                _cacheOptions.CatalogTtl,
                _ => LoadActiveFromDatabaseAsync(
                    directionId,
                    cancellationToken),
                cancellationToken);
        }

        return await _cache.GetOrCreateAsync(
            CacheKeys.Catalog("catalog:active-programs"),
            _cacheOptions.CatalogTtl,
            _ => LoadActiveFromDatabaseAsync(
                null,
                cancellationToken),
            cancellationToken);
    }

    /// <summary>Исходная выборка активных программ (бывшее тело GetActiveAsync).</summary>
    private async Task<List<ProgramDto>> LoadActiveFromDatabaseAsync(
        int? directionId,
        CancellationToken cancellationToken)
    {
        var query = _context.it_programs
            .AsNoTracking()
            .Where(x => x.is_active == true)
            .AsQueryable();

        if (directionId.HasValue)
        {
            query = query.Where(
                x => x.direction_id == directionId.Value);
        }

        return await query
            .Select(x => new ProgramDto
            {
                Id = x.it_programs_id,
                DirectionId = x.direction_id,
                Name = x.name,
                Description = x.description,
                IsActive = x.is_active
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ProgramDto>> SearchAsync(
        string? search,
        int? directionId,
        CancellationToken cancellationToken)
    {
        // БЕЗ КЭША ОСОЗНАННО: поисковая подстрока меняется на каждую
        // клавишу — каждая комбинация (search, directionId) это новый
        // одноразовый ключ. Кэш одноразовых значений — утечка памяти
        // KeyDB под значения, которые никто не прочитает.
        var query = _context.it_programs
            .AsNoTracking()
            .AsQueryable();

        if (directionId.HasValue)
        {
            query = query.Where(
                x => x.direction_id == directionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.name != null &&
                x.name.ToLower().Contains(search.ToLower()));
        }

        return await query
            .Select(x => new ProgramDto
            {
                Id = x.it_programs_id,
                DirectionId = x.direction_id,
                Name = x.name,
                Description = x.description,
                IsActive = x.is_active
            })
            .ToListAsync(cancellationToken);
    }
}