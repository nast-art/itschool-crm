using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Contracts;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис раздела «Договоры» — вся бизнес-логика: валидация,
/// транзакционная привязка к взаимодействию, аудит.
/// </summary>
/// <remarks>
/// Стилистика полностью повторяет DirectionService:
///   • ArgumentException — валидация, InvalidOperationException — конфликт,
///     null в UpdateAsync — «не найдено»;
///   • аудит — через IAuditService.WriteAsync, прямой работы с audit_logs нет;
///   • CancellationToken протаскивается во все вызовы EF.
///
/// Имена DbSet'ов/полей — по схеме БД: contracts, interactions,
/// universities, it_products, it_programs.
///
/// ВАЖНО про даты: даты приходят от фронта как «yyyy-MM-dd» (date-поля
/// формы) и читаются фронтом как UTC без суффикса Z. Поэтому храним
/// значение КАК ЕСТЬ, без сдвига часовых поясов — конвенция всего
/// приложения.
///
/// КЭШИРОВАНИЕ (KeyDB, cache-aside):
///   • GetAllAsync — реестр под общим ключом catalog:contracts:v{catalog:version}.
///     Реестр НЕ фильтруется по university_managers (по замыслу раздела,
///     как справочники) — скоп не нужен;
///   • GetInteractionOptionsAsync — общий ключ catalog:contract-interaction-options.
///     Его состав меняется и при записи в interactions — поэтому ключ
///     версионирован счётчиком interaction:version, а не catalog:version;
///   • CreateAsync/UpdateAsync — инвалидируют ОБА счётчика: создание договора
///     с привязкой меняет и реестр, и взаимодействие (появился contract_id,
///     в таблице вузов засветится «№ Договора»), и опции селекта.
/// </remarks>
public class ContractService : IContractService
{
    private readonly CrmDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    public ContractService(
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
    /// Сброс кэша после записи в договоры. Два счётчика:
    /// catalog:version — реестр договоров; interaction:version — взаимодействия
    /// (привязка contract_id видна в таблице вузов и карточках) и опции селекта.
    /// </summary>
    private async Task InvalidateContractsAsync(CancellationToken cancellationToken)
    {
        await _cache.BumpVersionAsync(CacheKeys.CatalogVersion, cancellationToken);
        await _cache.BumpVersionAsync(CacheKeys.InteractionVersionKey, cancellationToken);
    }

    /// <summary>
    /// Реестр договоров. Вузы и ПО агрегируются из взаимодействий
    /// (interactions.contract_id): один договор — несколько продуктов,
    /// поэтому списки с DISTINCT. Реестр общий для всех авторизованных
    /// (как справочники Products/Directions).
    /// </summary>
    public async Task<List<ContractDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Catalog("catalog:contracts"),
            _cacheOptions.CatalogTtl,
            _ => LoadRegistryFromDatabaseAsync(cancellationToken),
            cancellationToken);
    }

    /// <summary>Загрузка реестра из БД (промах кэша). Две выборки без N+1.</summary>
    private async Task<List<ContractDto>> LoadRegistryFromDatabaseAsync(CancellationToken cancellationToken)
    {
        var contracts = await _context.contracts
            .AsNoTracking()
            .OrderByDescending(x => x.signed_at)
            .ThenByDescending(x => x.contracts_id)
            .ToListAsync(cancellationToken);

        var contractIds = contracts.Select(c => c.contracts_id).ToList();

        // Связи всех найденных договоров одним запросом (без N+1).
        // Имена вузов: приоритет у короткого (short_name) — тот же
        // принцип отображения, что на всём фронте.
        var links = await GetLinksAsync(contractIds, cancellationToken);

        var linksByContract = links
            .GroupBy(l => l.ContractId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return contracts.Select(c =>
        {
            linksByContract.TryGetValue(c.contracts_id, out var contractLinks);
            contractLinks ??= new List<ContractLinkRow>();

            return new ContractDto
            {
                Id = c.contracts_id,
                ContractNumber = c.contract_number,
                SignedAt = c.signed_at,
                ValidUntil = c.valid_until,
                Status = c.status,
                Comment = c.comment,
                UniversityIds = DistinctIds(contractLinks.Select(l => l.UniversityId)),
                UniversityNames = DistinctNames(contractLinks.Select(l => l.UniversityName)),
                ProductIds = DistinctIds(contractLinks.Select(l => l.ProductId)),
                ProductNames = DistinctNames(contractLinks.Select(l => l.ProductName)),
            };
        }).ToList();
    }

    /// <summary>
    /// Взаимодействия БЕЗ договора — список для селекта «Вуз / ПО»
    /// в диалоге «Добавить договор».
    /// </summary>
    /// <remarks>
    /// Ключ версионирован счётчиком interaction:version, а не catalog:version —
    /// состав списка зависит от состояния взаимодействий (привязка/создание).
    /// Оба события инвалидируют interaction:version — кэш сбросится в обоих случаях.
    /// </remarks>
    public async Task<List<ContractInteractionOptionDto>> GetInteractionOptionsAsync(CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync(
            CacheKeys.Catalog("catalog:contract-interaction-options"),
            _cacheOptions.CatalogTtl,
            _ => LoadInteractionOptionsFromDatabaseAsync(cancellationToken),
            cancellationToken);
    }

    /// <summary>Загрузка опций из БД (промах кэша).</summary>
    private async Task<List<ContractInteractionOptionDto>> LoadInteractionOptionsFromDatabaseAsync(
        CancellationToken cancellationToken)
    {
        return await (
                from i in _context.interactions.AsNoTracking()
                join u in _context.universities
                    on i.university_id equals u.universities_id
                join p in _context.it_products
                    on i.product_id equals p.it_products_id into pg
                from p in pg.DefaultIfEmpty()
                join pr in _context.it_programs
                    on i.program_id equals pr.it_programs_id into prg
                from pr in prg.DefaultIfEmpty()
                where i.contract_id == null
                orderby i.interactions_id
                select new ContractInteractionOptionDto
                {
                    Id = i.interactions_id,
                    UniversityName = u.short_name != null && u.short_name != "" ? u.short_name : u.name,
                    ProductName = p != null ? p.name : null,
                    ProgramName = pr != null ? pr.name : null,
                })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Создание договора. При передаче interactionId договор СРАЗУ привязывается
    /// к взаимодействию — обе записи делаются в ОДНОЙ транзакции: либо создаётся
    /// и договор, и связь, либо ничего (нет «висячих» договоров при ошибке привязки).
    /// </summary>
    public async Task<ContractDto> CreateAsync(
        ContractCreateDto dto,
        CancellationToken cancellationToken)
    {
        // --- Валидация (ArgumentException, как в DirectionService) ---
        if (string.IsNullOrWhiteSpace(dto.ContractNumber))
        {
            throw new ArgumentException("Укажите номер договора.");
        }

        // Срок раньше подписания — невозможное состояние.
        // Клиент дублирует это атрибутом min у date-поля;
        // источник истины — сервер.
        if (dto.ValidUntil.HasValue
            && dto.SignedAt.HasValue
            && dto.ValidUntil < dto.SignedAt)
        {
            throw new ArgumentException(
                "Срок действия договора не может быть раньше даты подписания.");
        }

        // Уникальность номера (contract_number unique в схеме БД).
        // Проверка ДО вставки — отдаём понятный конфликт вместо
        // сырого исключения нарушения unique-констрейнта.
        var number = dto.ContractNumber.Trim();

        var numberExists = await _context.contracts.AnyAsync(
            x => x.contract_number == number,
            cancellationToken);

        if (numberExists)
        {
            throw new InvalidOperationException(
                $"Договор с номером «{number}» уже существует.");
        }

        // --- Опциональная привязка к взаимодействию ---
        interaction? linkedInteraction = null;

        if (dto.InteractionId.HasValue)
        {
            linkedInteraction = await _context.interactions
                .FirstOrDefaultAsync(i => i.interactions_id == dto.InteractionId.Value, cancellationToken);

            if (linkedInteraction == null)
            {
                throw new ArgumentException("Выбранное взаимодействие не найдено.");
            }

            if (linkedInteraction.contract_id.HasValue)
            {
                // Гонка двух менеджеров, привязывающих одно взаимодействие,
                // закрыта этой проверкой + транзакцией
                throw new InvalidOperationException(
                    "Выбранное взаимодействие уже привязано к другому договору.");
            }
        }

        var contract = new contract
        {
            contract_number = number,
            signed_at = dto.SignedAt,
            valid_until = dto.ValidUntil,
            // Статус по умолчанию совпадает с дефолтом формы на фронте
            status = string.IsNullOrWhiteSpace(dto.Status) ? "На подписании" : dto.Status.Trim(),
            comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
        };

        // Транзакция: договор + привязка — атомарно. Аудит пишем
        // после коммита (WriteAsync сам сохраняет свой контекст).
        await using var tx = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.contracts.Add(contract);
        await _context.SaveChangesAsync(cancellationToken);

        if (linkedInteraction != null)
        {
            linkedInteraction.contract_id = contract.contracts_id;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        // oldData у создания отсутствует; снимок и actor — внутри IAuditService
        await _auditService.WriteAsync(
            "CREATE",
            "contract",
            contract.contracts_id,
            null,
            new
            {
                contract.contracts_id,
                contract.contract_number,
                contract.signed_at,
                contract.valid_until,
                contract.status,
                contract.comment,
                dto.InteractionId,
            },
            cancellationToken);

        // Договор появился в реестре; если была привязка, у взаимодействия
        // появился contract_id (таблица вузов, карточки, отчёты), а привязанное
        // взаимодействие выбыло из опций селекта. Оба счётчика закрывают все случаи.
        await InvalidateContractsAsync(cancellationToken);

        // Сборка напрямую из БД, а не из кэша: свежая запись обязана быть
        // в ответе, кэш сброшен — следующий GET заполнит его заново.
        return await BuildDtoAsync(contract.contracts_id, cancellationToken);
    }

    /// <summary>
    /// Редактирование реквизитов. Вуз/ПО НЕ меняются здесь: состав договора
    /// определяется привязкой взаимодействий (interactions.contract_id).
    /// null = договор не найден (контроллер отдаёт 404).
    /// </summary>
    public async Task<ContractDto?> UpdateAsync(
        int id,
        ContractUpdateDto dto,
        CancellationToken cancellationToken)
    {
        var contract = await _context.contracts
            .FirstOrDefaultAsync(x => x.contracts_id == id, cancellationToken);

        if (contract is null)
        {
            return null;
        }

        // --- Валидация (ArgumentException) ---
        if (string.IsNullOrWhiteSpace(dto.ContractNumber))
        {
            throw new ArgumentException("Укажите номер договора.");
        }

        if (dto.ValidUntil.HasValue
            && dto.SignedAt.HasValue
            && dto.ValidUntil < dto.SignedAt)
        {
            throw new ArgumentException(
                "Срок действия договора не может быть раньше даты подписания.");
        }

        // Уникальность номера — с исключением самого себя
        var number = dto.ContractNumber.Trim();

        var numberExists = await _context.contracts.AnyAsync(
            x => x.contracts_id != id && x.contract_number == number,
            cancellationToken);

        if (numberExists)
        {
            throw new InvalidOperationException(
                $"Договор с номером «{number}» уже существует.");
        }

        // Снимок «до» для аудита — сериализуем ДО изменения
        var oldSnapshot = new
        {
            contract.contracts_id,
            contract.contract_number,
            contract.signed_at,
            contract.valid_until,
            contract.status,
            contract.comment,
        };

        contract.contract_number = number;
        contract.signed_at = dto.SignedAt;
        contract.valid_until = dto.ValidUntil;
        // Пустой статус в запросе означает «оставить как было»
        contract.status = string.IsNullOrWhiteSpace(dto.Status) ? contract.status : dto.Status.Trim();
        contract.comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.WriteAsync(
            "UPDATE",
            "contract",
            contract.contracts_id,
            oldSnapshot,
            new
            {
                contract.contracts_id,
                contract.contract_number,
                contract.signed_at,
                contract.valid_until,
                contract.status,
                contract.comment,
            },
            cancellationToken);

        // Реквизиты (номер, статус, сроки) видны в реестре, а номер —
        // в таблице вузов и отчётах (колонка «ℓ Договора» строится из contract)
        await InvalidateContractsAsync(cancellationToken);

        return await BuildDtoAsync(contract.contracts_id, cancellationToken);
    }

    /// <summary>
    /// Связи «договор -> вузы/продукты» через взаимодействия.
    /// Один запрос на пачку договоров (без N+1). Явные join'ы вместо
    /// навигаций — чтобы не зависеть от наличия навигационных свойств
    /// в сущностях. LEFT JOIN к продуктам: product_id nullable.
    /// </summary>
    private async Task<List<ContractLinkRow>> GetLinksAsync(
        List<int> contractIds,
        CancellationToken cancellationToken)
    {
        if (contractIds.Count == 0)
        {
            return new List<ContractLinkRow>();
        }

        return await (
                from i in _context.interactions.AsNoTracking()
                join u in _context.universities
                    on i.university_id equals u.universities_id
                join p in _context.it_products
                    on i.product_id equals p.it_products_id into pg
                from p in pg.DefaultIfEmpty()
                where i.contract_id.HasValue
                      && contractIds.Contains(i.contract_id.Value)
                select new ContractLinkRow(
                    i.contract_id!.Value,
                    i.university_id,
                    u.short_name != null && u.short_name != "" ? u.short_name : u.name,
                    i.product_id,
                    p != null ? p.name : null))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Сборка DTO одного договора с агрегированными вузами/продуктами.
    /// Идёт напрямую в БД, минуя кэш реестра — в POST/PUT ответ обязан
    /// содержать только что записанные данные.
    /// </summary>
    private async Task<ContractDto> BuildDtoAsync(
        int contractId,
        CancellationToken cancellationToken)
    {
        var contract = await _context.contracts
            .AsNoTracking()
            .FirstAsync(x => x.contracts_id == contractId, cancellationToken);

        var links = await GetLinksAsync(new List<int> { contractId }, cancellationToken);

        return new ContractDto
        {
            Id = contract.contracts_id,
            ContractNumber = contract.contract_number,
            SignedAt = contract.signed_at,
            ValidUntil = contract.valid_until,
            Status = contract.status,
            Comment = contract.comment,
            UniversityIds = DistinctIds(links.Select(l => l.UniversityId)),
            UniversityNames = DistinctNames(links.Select(l => l.UniversityName)),
            ProductIds = DistinctIds(links.Select(l => l.ProductId)),
            ProductNames = DistinctNames(links.Select(l => l.ProductName)),
        };
    }

    // Агрегирование nullable-списков: отбрасываем null, DISTINCT — в одну строку.
    // Повторяющиеся .Where().Select().Distinct() вынесены сюда, чтобы не
    // дублировать цепочки вызовов в сборке DTO.
    private static List<int> DistinctIds(IEnumerable<int?> ids) => ids.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();

    private static List<string> DistinctNames(IEnumerable<string?> names) => names.Where(n => n != null).Select(n => n!).Distinct().ToList();

    /// <summary>
    /// Строка связи договора с вузом/продуктом (промежуточная проекция,
    /// не выходит за пределы сервиса). UniversityId nullable: в модели
    /// interactions.university_id объявлен nullable, EF не даёт передать его как int.
    /// </summary>
    private sealed record ContractLinkRow(
        int ContractId,
        int? UniversityId,
        string? UniversityName,
        int? ProductId,
        string? ProductName);
}