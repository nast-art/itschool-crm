using ITSchoolCRM.API.DTOs.Reports;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IReportService
{
    Task<List<ReportRowDto>> GetRowsAsync(ReportFilterDto filter, CancellationToken cancellationToken);

    Task<byte[]> GenerateXlsAsync(ReportFilterDto filter, CancellationToken cancellationToken);

    Task<byte[]> GenerateXlsxAsync(ReportFilterDto filter, CancellationToken cancellationToken);

    Task<byte[]> GeneratePdfAsync(ReportFilterDto filter, CancellationToken cancellationToken);

    Task<byte[]> GenerateJsonAsync(ReportFilterDto filter, CancellationToken cancellationToken);

    Task<StatisticsDto> GetStatisticsAsync(ReportFilterDto filter, CancellationToken cancellationToken);

    Task<byte[]> GenerateStatisticsPngAsync(ReportFilterDto filter, CancellationToken cancellationToken);

    Task<byte[]> GenerateStatisticsPdfAsync(ReportFilterDto filter, CancellationToken cancellationToken);
}