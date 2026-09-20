using System.Text.Json;
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

public class ReportService : IReportService
{
    private readonly CrmDbContext _context;
    private readonly IUserAccessService _accessService;

    public ReportService(
        CrmDbContext context,
        IUserAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<List<ReportRowDto>>
        GetRowsAsync(
            ReportFilterDto filter,
            CancellationToken cancellationToken)
    {
        var accessibleIds =
            _accessService
                .GetAccessibleUniversityIds();

        var query =
            _context.interactions
                .AsNoTracking()
                .Where(x =>
                    x.university_id.HasValue &&
                    accessibleIds.Contains(
                        x.university_id.Value))
                .AsQueryable();

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

        if (filter.ResponsibleUserIds.Count > 0)
        {
            query =
                query.Where(
                    x =>
                        x.manager_id.HasValue &&
                        filter.ResponsibleUserIds.Contains(
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

                    // Колонки full_name нет: ФИО собирается из частей.
                    ResponsibleName =
                        x.manager == null
                            ? null
                            : x.manager.middle_name == null
                                ? x.manager.last_name + " " + x.manager.first_name
                                : x.manager.last_name + " " + x.manager.first_name + " " + x.manager.middle_name,

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

        var headers =
            new List<string>();

        if (filter.IncludeUniversity)
        {
            headers.Add(
                "Наименование ВУЗа");
        }

        if (filter.IncludeDirection)
        {
            headers.Add(
                "ИТ-направление");
        }

        if (filter.IncludeProduct)
        {
            headers.Add(
                "ИТ-продукт");
        }

        if (filter.IncludeStatus)
        {
            headers.Add(
                "Статус работы с вузом");
        }

        if (filter.IncludeResponsible)
        {
            headers.Add(
                "Ответственный");
        }

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

            var row =
                sheet.CreateRow(
                    rowIndex + 1);

            var column =
                0;

            if (filter.IncludeUniversity)
            {
                row.CreateCell(column++)
                    .SetCellValue(
                        source.UniversityName ??
                        string.Empty);
            }

            if (filter.IncludeDirection)
            {
                row.CreateCell(column++)
                    .SetCellValue(
                        source.DirectionName ??
                        string.Empty);
            }

            if (filter.IncludeProduct)
            {
                row.CreateCell(column++)
                    .SetCellValue(
                        source.ProductName ??
                        string.Empty);
            }

            if (filter.IncludeStatus)
            {
                row.CreateCell(column++)
                    .SetCellValue(
                        source.StatusName ??
                        string.Empty);
            }

            if (filter.IncludeResponsible)
            {
                row.CreateCell(column++)
                    .SetCellValue(
                        source.ResponsibleName ??
                        string.Empty);
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

    private static List<string> GetHeaders(
        ReportFilterDto filter)
    {
        var result =
            new List<string>();

        if (filter.IncludeUniversity)
        {
            result.Add(
                "Наименование ВУЗа");
        }

        if (filter.IncludeDirection)
        {
            result.Add(
                "ИТ-направление");
        }

        if (filter.IncludeProduct)
        {
            result.Add(
                "ИТ-продукт");
        }

        if (filter.IncludeStatus)
        {
            result.Add(
                "Статус работы с вузом");
        }

        if (filter.IncludeResponsible)
        {
            result.Add(
                "Ответственный");
        }

        return result;
    }

    private static List<string> GetValues(
        ReportRowDto row,
        ReportFilterDto filter)
    {
        var result =
            new List<string>();

        if (filter.IncludeUniversity)
        {
            result.Add(
                row.UniversityName ??
                string.Empty);
        }

        if (filter.IncludeDirection)
        {
            result.Add(
                row.DirectionName ??
                string.Empty);
        }

        if (filter.IncludeProduct)
        {
            result.Add(
                row.ProductName ??
                string.Empty);
        }

        if (filter.IncludeStatus)
        {
            result.Add(
                row.StatusName ??
                string.Empty);
        }

        if (filter.IncludeResponsible)
        {
            result.Add(
                row.ResponsibleName ??
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