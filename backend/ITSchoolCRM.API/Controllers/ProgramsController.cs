using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Programs;
using ITSchoolCRM.API.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgramsController : ControllerBase
{
    private readonly IProgramService _service;

    public ProgramsController(IProgramService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<ProgramDto>>> GetAll([FromQuery] int? directionId, CancellationToken cancellationToken)
    {
        var programs = await _service.GetAllAsync(directionId, cancellationToken);

        return Ok(programs);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<ProgramDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var program = await _service.GetByIdAsync(id, cancellationToken);

        if (program is null)
        {
            return NotFound();
        }

        return Ok(program);
    }

    [HttpGet("active")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<ProgramDto>>> GetActive([FromQuery] int? directionId, CancellationToken cancellationToken)
    {
        var programs = await _service.GetActiveAsync(directionId, cancellationToken);

        return Ok(programs);
    }

    [HttpGet("search")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<ProgramDto>>> Search([FromQuery] string? search, [FromQuery] int? directionId, CancellationToken cancellationToken)
    {
        var programs = await _service.SearchAsync(search, directionId, cancellationToken);

        return Ok(programs);
    }

    [HttpPost]
    [Authorize(Policy = Policies.ManagerAccess)]
    public async Task<ActionResult<ProgramDto>> Create([FromBody] CreateProgramDto dto, CancellationToken cancellationToken)
    {
        var program = await _service.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = program.Id }, program);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.ManagerAccess)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProgramDto dto, CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, dto, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}