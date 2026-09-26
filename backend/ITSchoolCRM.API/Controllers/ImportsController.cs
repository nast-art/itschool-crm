using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Imports;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportsController : ControllerBase
{
    private readonly IImportService _service;

    public ImportsController(IImportService service)
    {
        _service = service;
    }

    [HttpGet("mapping")]
    [Authorize(Policy = Policies.AdminAccess)]
    public ActionResult<Dictionary<string, string>> GetMapping()
    {
        return Ok(_service.GetAvailableMappingFields());
    }

    [HttpPost("excel")]
    [Authorize(Policy = Policies.AdminAccess)]
    [RequestSizeLimit(50L * 1024L * 1024L)]
    public async Task<ActionResult<ImportBatchDto>> ImportExcel(IFormFile file, [FromForm] string mapping, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest("Файл не выбран.");
        }

        if (string.IsNullOrWhiteSpace(mapping))
        {
            return BadRequest("Mapping не указан.");
        }

        ImportMappingDto? mappingDto;

        try
        {
            mappingDto = System.Text.Json.JsonSerializer.Deserialize<ImportMappingDto>(
                mapping,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
        catch
        {
            return BadRequest("Некорректный JSON mapping.");
        }

        if (mappingDto is null)
        {
            return BadRequest("Mapping не указан.");
        }

        var batch = await _service.ImportExcelAsync(file, mappingDto, cancellationToken);

        return Ok(batch);
    }

    [HttpPost("json")]
    [Authorize(Policy = Policies.AdminAccess)]
    [RequestSizeLimit(50L * 1024L * 1024L)]
    public async Task<ActionResult<ImportBatchDto>> ImportJson(IFormFile file, CancellationToken cancellationToken)
    {
        var batch = await _service.ImportJsonAsync(file, cancellationToken);

        return Ok(batch);
    }

    [HttpGet("batches")]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<ActionResult<List<ImportBatchDto>>> GetBatches(CancellationToken cancellationToken)
    {
        var batches = await _service.GetBatchesAsync(cancellationToken);

        return Ok(batches);
    }

    [HttpGet("batches/{id:int}")]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<ActionResult<ImportBatchDto>> GetBatch(int id, CancellationToken cancellationToken)
    {
        var batch = await _service.GetBatchByIdAsync(id, cancellationToken);

        if (batch is null)
        {
            return NotFound();
        }

        return Ok(batch);
    }
}