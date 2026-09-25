using System.Text.Json;
using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Reports;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис отчётов и статистики.
///
/// КЭШИРОВАНИЕ (KeyDB, cache-aside) — только GetStatisticsAsync:
/// дашборд и страница визуализации выполняют 4–5 вызовов
/// статистики подряд (основной агрегат + фасетные счётчики
/// для меню фильтров), и каждый раньше пересчитывал выборку
/// целиком. Теперь повторные вызовы с тем же фильтром
/// отдаются из кэша за миллисекунды.
///
/// Ключ статистики: scope (full для manager/admin, KeycloakUserId
/// для менеджера — выборка фильтруется по university_managers)
/// + хэш фильтра (FilterHash): каждая комбинация дат/вузов/
/// направлений/продуктов/менеджеров получает свой ключ.
///
/// ИНВАЛИДАЦИЯ: агрегаты версионированы счётчиком
/// interaction:version — тот же, что сбрасывают ChangeStatusAsync,
/// AddCommentAsync, UpdateAsync, CreateAsync в InteractionService,
/// а также импорт каталога. Перевод статуса на фронте делает
/// INCR — и все закэшированные агрегаты пересчитаются при
/// следующем запросе. Короткий TTL (StatisticsTtl, 60 сек)
/// страхует на случай изменений вне CRM (прямые правки БД).
///
/// СОЗНАТЕЛЬНО БЕЗ КЭША:
///   - GetRowsAsync и все Generate*Async — файл должен
///     формироваться по данным на момент запроса; кэш строк
///     между экспортом и экраном создавал бы рассинхрон
///     («на экране одно, в файле другое»);
///   - GenerateStatisticsPngAsync/PdfAsync — картинки
///     собираются из агрегатов, которые уже кэшируются.
/// </summary>
public class ReportService : IReportService
{
    private readonly CrmDbContext _context;
    private readonly IUserAccessService _accessService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    // Ключи колонок — РОВНО те, что шлёт фронтенд
    // (ALL_COLUMNS в ReportsPage.jsx). Порядок здесь определяет
    // порядок колонок в файлах xls/xlsx/pdf.
    private const string ColUniversity = "university";
    private const string ColDirection = "direction";
    private const string ColProduct = "product";
    private const string ColStatus = "status";
    private const string ColManager = "manager";
    private const string ColVendor = "vendor";
    private const string ColContractNumber = "contractNumber";

    public ReportService(
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
    /// Колонка включена в отчёт, если она есть в списке Columns фильтра.
    /// Пустой список Columns трактуем как «все колонки» — чтобы прямые
    /// вызовы API без поля columns не получали пустой файл.
    /// </summary>
    private static bool HasColumn(
        ReportFilterDto filter,
        string key)
    {
        return filter.Columns == null ||
               filter.Columns.Count == 0 ||
               filter.Columns.Contains(key);
    }

    /// <summary>
    /// Скоп ключа кэша статистики: "full" для manager/admin
    /// (видят все вузы — общий кэш уместен), KeycloakUserId
    /// для менеджера по вузам (его выборка — только свои вузы).
    /// </summary>
    private string? CurrentCacheScope =>
        _accessService.HasFullAccess()
            ? CacheKeys.FullAccessScope
            : _currentUserService.KeycloakUserId;

    public async Task<List<ReportRowDto>>
        GetRowsAsync(
            ReportFilterDto filter,
            CancellationToken cancellationToken)
    {
        var accessibleIds =
            _accessService
                .GetAccessibleUniversityIds();

        // Базовая выборка: только вузы, доступные текущему пользователю
        // (разграничение по данным через university_managers — п. 11 ТЗ).
        // Фронтенд это НЕ дублирует.
        var query =
            _context.interactions
                .AsNoTracking()
                .Where(x =>
                    x.university_id.HasValue &&
                    accessibleIds.Contains(
                        x.university_id.Value))
                .AsQueryable();

        // Имена свойств фильтра совпадают с json-телом запроса
        // (camelCase-политика по умолчанию). Фильтр «не работает» —
        // проверять контракт DTO, не этот метод.
        if (filter.DateFrom.HasValue)
        {
            query =
                query.Where(
                    x =>
                        x.created_at >=
                        filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            // Конец периода включительно: всё, что создано
            // ДО начала следующего дня
            var dateTo =
                filter.DateTo.Value
                    .Date
                    .AddDays(1);

            query =
                query.Where(
                    x =>
                        x.created_at <
                        dateTo);
        }

        if (filter.UniversityIds.Count > 0)
        {
            query =
                query.Where(
                    x =>
                        x.university_id.HasValue &&
                        filter.UniversityIds
                            .Contains(
                                x.university_id.Value));
        }

        if (filter.DirectionIds.Count > 0)
        {
            query =
                query.Where(
                    x =>
                        x.program != null &&
                        x.program.direction_id.HasValue &&
                        filter.DirectionIds.Contains(
                            x.program.direction_id.Value));
        }

        if (filter.ProductIds.Count > 0)
        {
            query =
                query.Where(
                    x =>
                        x.product_id.HasValue &&
                        filter.ProductIds.Contains(
                            x.product_id.Value));
        }

        // ИСПРАВЛЕНО: было filter.ResponsibleUserIds — фронт слал managerIds,
        // свойство оставалось пустым и фильтр по ответственному не работал.
        if (filter.ManagerIds.Count > 0)
        {
            query =
                query.Where(
                    x =>
                        x.manager_id.HasValue &&
                        filter.ManagerIds.Contains(
                            x.manager_id.Value));
        }

        return await query
            .OrderByDescending(
                x => x.created_at)
            .Select(
                x => new ReportRowDto
                {
                    InteractionId =
                        x.interactions_id,

                    UniversityName =
                        x.university != null
                            ? x.university.name
                            : null,

                    DirectionName =
                        x.program != null &&
                        x.program.direction != null
                            ? x.program.direction.name
                            : null,

                    ProductName =
                        x.product != null
                            ? x.product.name
                            : null,

                    StatusName =
                        x.current_status != null
                            ? x.current_status.name
                            : null,

                    // ФИО собирается из частей: в БД колонки full_name
                    // больше нет (удалена миграцией из ТЗ).
                    // Порядок русского ФИО: фамилия → имя → отчество.
                    ResponsibleName =
                        x.manager == null
                            ? null
                            : x.manager.middle_name == null
                                ? x.manager.last_name + " " + x.manager.first_name
                                : x.manager.last_name + " " + x.manager.first_name + " " + x.manager.middle_name,

                    // Колонка «Вендор» — поле vendor справочника it_products
                    // (по схеме БД из ТЗ). Совпадает с фронтендом:
                    // productById[i.productId].vendor.
                    VendorName =
                        x.product != null
                            ? x.product.vendor
                            : null,

                    // Колонка «№ Договора» — через interactions.contract_id
                    // → contracts.contract_number. Навигационное свойство
                    // contract должно быть настроено в CrmDbContext
                    // (см. примечание после кода).
                    ContractNumber =
                        x.contract != null
                            ? x.contract.contract_number
                            : null,

                    CreatedAt =
                        x.created_at,

                    UpdatedAt =
                        x.updated_at
                })
            .ToListAsync(
                cancellationToken);
    }

    public async Task<byte[]> GenerateXlsAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        var rows =
            await GetRowsAsync(
                filter,
                cancellationToken);

        IWorkbook workbook =
            new HSSFWorkbook();

        FillWorkbook(
            workbook,
            rows,
            filter);

        using var stream =
            new MemoryStream();

        workbook.Write(stream);

        return stream.ToArray();
    }

    public async Task<byte[]> GenerateXlsxAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        var rows =
            await GetRowsAsync(
                filter,
                cancellationToken);

        IWorkbook workbook =
            new XSSFWorkbook();

        FillWorkbook(
            workbook,
            rows,
            filter);

        using var stream =
            new MemoryStream();

        workbook.Write(stream);

        return stream.ToArray();
    }

    private static void FillWorkbook(
        IWorkbook workbook,
        List<ReportRowDto> rows,
        ReportFilterDto filter)
    {
        var sheet =
            workbook.CreateSheet(
                "Отчёт");

        var headerStyle =
            workbook.CreateCellStyle();

        var headerFont =
            workbook.CreateFont();

        headerFont.IsBold =
            true;

        headerStyle.SetFont(
            headerFont);

        // Единый источник правды о составе колонок (GetHeaders/GetValues)
        // для xls, xlsx и pdf — порядок не может разойтись между форматами.
        var headers =
            GetHeaders(
                filter);

        var headerRow =
            sheet.CreateRow(0);

        for (
            var i = 0;
            i < headers.Count;
            i++)
        {
            var cell =
                headerRow.CreateCell(i);

            cell.SetCellValue(
                headers[i]);

            cell.CellStyle =
                headerStyle;
        }

        for (
            var rowIndex = 0;
            rowIndex < rows.Count;
            rowIndex++)
        {
            var source =
                rows[rowIndex];

            var values =
                GetValues(
                    source,
                    filter);

            var row =
                sheet.CreateRow(
                    rowIndex + 1);

            for (
                var column = 0;
                column < values.Count;
                column++)
            {
                row.CreateCell(column)
                    .SetCellValue(
                        values[column]);
            }
        }

        for (
            var i = 0;
            i < headers.Count;
            i++)
        {
            sheet.AutoSizeColumn(i);
        }
    }

    public async Task<byte[]> GeneratePdfAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        var rows =
            await GetRowsAsync(
                filter,
                cancellationToken);

        var document =
            Document.Create(
                container =>
                {
                    container.Page(
                        page =>
                        {
                            page.Size(
                                PageSizes.A4.Landscape());

                            page.Margin(25);

                            page.DefaultTextStyle(
                                x =>
                                    x.FontSize(9));

                            page.Header()
                                .Text(
                                    "Отчёт по взаимодействиям с ВУЗами")
                                .FontSize(18)
                                .Bold();

                            page.Content()
                                .PaddingTop(15)
                                .Table(
                                    table =>
                                    {
                                        var columns =
                                            GetColumnCount(
                                                filter);

                                        table.ColumnsDefinition(
                                            definition =>
                                            {
                                                for (
                                                    var i = 0;
                                                    i < columns;
                                                    i++)
                                                {
                                                    definition
                                                        .RelativeColumn();
                                                }
                                            });

                                        table.Header(
                                            header =>
                                            {
                                                foreach (
                                                    var title
                                                        in GetHeaders(
                                                            filter))
                                                {
                                                    header.Cell()
                                                        .Background(
                                                            Colors.Grey.Lighten2)
                                                        .Padding(5)
                                                        .Text(
                                                            title)
                                                        .Bold();
                                                }
                                            });

                                        foreach (
                                            var row in rows)
                                        {
                                            foreach (
                                                var value
                                                    in GetValues(
                                                        row,
                                                        filter))
                                            {
                                                table.Cell()
                                                    .BorderBottom(
                                                        1)
                                                    .BorderColor(
                                                        Colors.Grey.Lighten2)
                                                    .Padding(5)
                                                    .Text(
                                                        value);
                                            }
                                        }
                                    });

                            page.Footer()
                                .AlignCenter()
                                .Text(
                                    $"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}");
                        });
                });

        return document.GeneratePdf();
    }

    private static int GetColumnCount(
        ReportFilterDto filter)
    {
        return GetHeaders(filter).Count;
    }

    // Единый источник правды о составе колонок.
    // Ключи — те, что шлёт фронтенд в columns (ALL_COLUMNS).
    // Порядок заголовков СТРОГО совпадает с порядком в GetValues.
    private static List<string> GetHeaders(
        ReportFilterDto filter)
    {
        var result =
            new List<string>();

        if (HasColumn(filter, ColUniversity))
        {
            result.Add(
                "Наименование ВУЗа");
        }

        if (HasColumn(filter, ColDirection))
        {
            result.Add(
                "ИТ-направление");
        }

        if (HasColumn(filter, ColProduct))
        {
            result.Add(
                "ИТ-продукт");
        }

        if (HasColumn(filter, ColStatus))
        {
            result.Add(
                "Статус работы с вузом");
        }

        if (HasColumn(filter, ColManager))
        {
            result.Add(
                "Ответственный");
        }

        if (HasColumn(filter, ColVendor))
        {
            result.Add(
                "Вендор");
        }

        if (HasColumn(filter, ColContractNumber))
        {
            result.Add(
                "№ Договора");
        }

        return result;
    }

    private static List<string> GetValues(
        ReportRowDto row,
        ReportFilterDto filter)
    {
        var result =
            new List<string>();

        if (HasColumn(filter, ColUniversity))
        {
            result.Add(
                row.UniversityName ??
                string.Empty);
        }

        if (HasColumn(filter, ColDirection))
        {
            result.Add(
                row.DirectionName ??
                string.Empty);
        }

        if (HasColumn(filter, ColProduct))
        {
            result.Add(
                row.ProductName ??
                string.Empty);
        }

        if (HasColumn(filter, ColStatus))
        {
            result.Add(
                row.StatusName ??
                string.Empty);
        }

        if (HasColumn(filter, ColManager))
        {
            result.Add(
                row.ResponsibleName ??
                string.Empty);
        }

        if (HasColumn(filter, ColVendor))
        {
            result.Add(
                row.VendorName ??
                string.Empty);
        }

        if (HasColumn(filter, ColContractNumber))
        {
            result.Add(
                row.ContractNumber ??
                string.Empty);
        }

        return result;
    }

    public async Task<byte[]> GenerateJsonAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        var rows =
            await GetRowsAsync(
                filter,
                cancellationToken);

        // Результирующий json (п. 4 «Требований к решению») —
        // расширенная привилегия администратора. Состав строк —
        // все поля ReportRowDto, включая ключи связей (InteractionId),
        // что позволяет сопоставить записи с файлами в storage (S3).
        return JsonSerializer.SerializeToUtf8Bytes(
            rows,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }

    public async Task<StatisticsDto>
        GetStatisticsAsync(
            ReportFilterDto filter,
            CancellationToken cancellationToken)
    {
        var scope = CurrentCacheScope;

        // Пользователь не аутентифицирован (эндпоинт под авторизацией,
        // но защищаемся): считаем без кэша — выборка всё равно пустая.
        if (scope is null)
        {
            return await ComputeStatisticsAsync(
                filter,
                cancellationToken);
        }

        // Хэш фильтра: каноническая JSON-сериализация → SHA-256,
        // первые 64 бита. Одна и та же комбинация дат/вузов/направлений/
        // продуктов/менеджеров даёт один ключ → попадание в кэш;
        // изменение любого поля — новый ключ → пересчёт.
        var filterHash =
            FilterHash.Compute(filter);

        // Ключ версионирован счётчиком interaction:version — инвалидация
        // происходит в InteractionService (перевод статуса, комментарий,
        // редактирование, создание) и в ImportService. Сам ReportService
        // ничего не пишет, ему сбрасывать нечего.
        return await _cache.GetOrCreateAsync(
            CacheKeys.Statistics(
                scope,
                filterHash),
            _cacheOptions.StatisticsTtl,
            _ => ComputeStatisticsAsync(
                filter,
                cancellationToken),
            cancellationToken);
    }

    /// <summary>
    /// Расчёт агрегатов (бывшее тело GetStatisticsAsync).
    /// GetRowsAsync сам фильтрует по university_managers — агрегаты
    /// корректны для текущего пользователя, поэтому ключ кэша
    /// обязан включать scope.
    /// </summary>
    private async Task<StatisticsDto> ComputeStatisticsAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        var rows =
            await GetRowsAsync(
                filter,
                cancellationToken);

        return new StatisticsDto
        {
            TotalInteractions =
                rows.Count,

            ByStatus =
                rows
                    .GroupBy(
                        x =>
                            x.StatusName ??
                            "Без статуса")
                    .OrderByDescending(
                        x => x.Count())
                    .Select(
                        x => new StatisticsItemDto
                        {
                            Name =
                                x.Key,

                            Count =
                                x.Count()
                        })
                    .ToList(),

            ByUniversity =
                rows
                    .GroupBy(
                        x =>
                            x.UniversityName ??
                            "Без ВУЗа")
                    .OrderByDescending(
                        x => x.Count())
                    .Select(
                        x => new StatisticsItemDto
                        {
                            Name =
                                x.Key,

                            Count =
                                x.Count()
                        })
                    .ToList(),

            ByDirection =
                rows
                    .GroupBy(
                        x =>
                            x.DirectionName ??
                            "Без направления")
                    .OrderByDescending(
                        x => x.Count())
                    .Select(
                        x => new StatisticsItemDto
                        {
                            Name =
                                x.Key,

                            Count =
                                x.Count()
                        })
                    .ToList(),

            ByProduct =
                rows
                    .GroupBy(
                        x =>
                            x.ProductName ??
                            "Без продукта")
                    .OrderByDescending(
                        x => x.Count())
                    .Select(
                        x => new StatisticsItemDto
                        {
                            Name =
                                x.Key,

                            Count =
                                x.Count()
                        })
                    .ToList()
        };
    }

    public async Task<byte[]>
        GenerateStatisticsPngAsync(
            ReportFilterDto filter,
            CancellationToken cancellationToken)
    {
        var statistics =
            await GetStatisticsAsync(
                filter,
                cancellationToken);

        return GenerateBarChart(
            "Взаимодействия по статусам",
            statistics.ByStatus);
    }

    public async Task<byte[]>
        GenerateStatisticsPdfAsync(
            ReportFilterDto filter,
            CancellationToken cancellationToken)
    {
        var statistics =
            await GetStatisticsAsync(
                filter,
                cancellationToken);

        var png =
            GenerateBarChart(
                "Взаимодействия по статусам",
                statistics.ByStatus);

        var document =
            Document.Create(
                container =>
                {
                    container.Page(
                        page =>
                        {
                            page.Size(
                                PageSizes.A4);

                            page.Margin(30);

                            page.Header()
                                .Text(
                                    "Статистика взаимодействий")
                                .FontSize(20)
                                .Bold();

                            page.Content()
                                .PaddingTop(20)
                                .Column(
                                    column =>
                                    {
                                        column.Item()
                                            .Text(
                                                $"Всего взаимодействий: {statistics.TotalInteractions}")
                                            .FontSize(14)
                                            .Bold();

                                        column.Item()
                                            .PaddingTop(20)
                                            .Image(
                                                png);
                                    });

                            page.Footer()
                                .AlignCenter()
                                .Text(
                                    $"Сформировано: {DateTime.Now:dd.MM.yyyy HH:mm}");
                        });
                });

        return document.GeneratePdf();
    }

    private static byte[] GenerateBarChart(
        string title,
        List<StatisticsItemDto> items)
    {
        const int width = 1400;
        const int height = 800;

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);

        canvas.Clear(SKColors.White);

        using var titleFont = new SKFont
        {
            Size = 34
        };

        using var titlePaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        using var titleBlob = SKTextBlob.Create(title, titleFont);

        canvas.DrawText(
            titleBlob,
            60,
            60,
            titlePaint);

        if (items.Count == 0)
        {
            using var emptyFont = new SKFont
            {
                Size = 28
            };

            using var emptyPaint = new SKPaint
            {
                Color = SKColors.DarkGray,
                IsAntialias = true
            };

            using var emptyBlob =
                SKTextBlob.Create(
                    "Нет данных",
                    emptyFont);

            canvas.DrawText(
                emptyBlob,
                60,
                130,
                emptyPaint);

            return EncodePng(bitmap);
        }

        var max = Math.Max(
            1,
            items.Max(x => x.Count));

        const float chartTop = 120;
        const float chartBottom = 700;
        const float chartLeft = 100;
        const float chartRight = 1340;

        var chartHeight =
            chartBottom - chartTop;

        var slotWidth =
            (chartRight - chartLeft) /
            items.Count;

        using var barPaint = new SKPaint
        {
            Color = SKColors.SteelBlue,
            IsAntialias = true
        };

        using var textFont = new SKFont
        {
            Size = 20
        };

        using var textPaint = new SKPaint
        {
            Color = SKColors.Black,
            IsAntialias = true
        };

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            var barHeight =
                chartHeight *
                item.Count /
                (float)max;

            var left =
                chartLeft +
                i * slotWidth +
                15;

            var right =
                chartLeft +
                (i + 1) * slotWidth -
                15;

            var top =
                chartBottom -
                barHeight;

            canvas.DrawRect(
                left,
                top,
                right,
                chartBottom,
                barPaint);

            var countText =
                item.Count.ToString();

            using var countBlob =
                SKTextBlob.Create(
                    countText,
                    textFont);

            canvas.DrawText(
                countBlob,
                left,
                top - 10,
                textPaint);

            var label =
                item.Name.Length > 18
                    ? item.Name[..18] + "..."
                    : item.Name;

            using var labelBlob =
                SKTextBlob.Create(
                    label,
                    textFont);

            canvas.DrawText(
                labelBlob,
                left,
                chartBottom + 30,
                textPaint);
        }

        return EncodePng(bitmap);
    }

    private static byte[] EncodePng(
        SKBitmap bitmap)
    {
        using var image =
            SKImage.FromBitmap(
                bitmap);

        using var data =
            image.Encode(
                SKEncodedImageFormat.Png,
                100);

        return data.ToArray();
    }
}