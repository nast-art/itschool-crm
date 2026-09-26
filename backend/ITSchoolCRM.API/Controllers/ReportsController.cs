using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Reports;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service)
    {
        _service = service;
    }

    [HttpPost("xls")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<IActionResult> Xls([FromBody] ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var file = await _service.GenerateXlsAsync(filter, cancellationToken);

        return File(
            file,
            "application/vnd.ms-excel",
            $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xls");
    }

    [HttpPost("xlsx")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<IActionResult> Xlsx([FromBody] ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var file = await _service.GenerateXlsxAsync(filter, cancellationToken);

        return File(
            file,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpPost("pdf")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<IActionResult> Pdf([FromBody] ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var file = await _service.GeneratePdfAsync(filter, cancellationToken);

        return File(
            file,
            "application/pdf",
            $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
    }

    [HttpPost("json")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<IActionResult> Json([FromBody] ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var file = await _service.GenerateJsonAsync(filter, cancellationToken);

        return File(
            file,
            "application/json",
            $"report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
    }

    [HttpPost("statistics")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<StatisticsDto>> Statistics([FromBody] ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var statistics = await _service.GetStatisticsAsync(filter, cancellationToken);

        return Ok(statistics);
    }

    [HttpPost("statistics/png")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<IActionResult> StatisticsPng([FromBody] ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var file = await _service.GenerateStatisticsPngAsync(filter, cancellationToken);

        return File(
            file,
            "image/png",
            $"statistics_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");
    }

    [HttpPost("statistics/pdf")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<IActionResult> StatisticsPdf([FromBody] ReportFilterDto filter, CancellationToken cancellationToken)
    {
        var file = await _service.GenerateStatisticsPdfAsync(filter, cancellationToken);

        return File(
            file,
            "application/pdf",
            $"statistics_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf");
    }
}