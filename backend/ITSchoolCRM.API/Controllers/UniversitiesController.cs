using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Universities;
using ITSchoolCRM.API.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UniversitiesController : ControllerBase
{
    private readonly IUniversityService _service;
    private readonly IUserAccessService _accessService;

    public UniversitiesController(IUniversityService service, IUserAccessService accessService)
    {
        _service = service;
        _accessService = accessService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<UniversityDto>>> GetAll(CancellationToken cancellationToken)
    {
        var universities = await _service.GetAllAsync(cancellationToken);

        return Ok(universities);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<UniversityDto>> GetById(int id, CancellationToken cancellationToken)
    {
        // Менеджер видит только закреплённые за ним вузы (152-ФЗ)
        var hasAccess = await _accessService.HasAccessToUniversityAsync(id, cancellationToken);

        if (!hasAccess)
        {
            return Forbid();
        }

        var university = await _service.GetByIdAsync(id, cancellationToken);

        if (university is null)
        {
            return NotFound();
        }

        return Ok(university);
    }

    [HttpGet("active")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<UniversityDto>>> GetActive(CancellationToken cancellationToken)
    {
        var universities = await _service.GetActiveAsync(cancellationToken);

        return Ok(universities);
    }

    [HttpGet("search")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<UniversityDto>>> Search([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var universities = await _service.SearchAsync(search, cancellationToken);

        return Ok(universities);
    }

    [HttpPost]
    [Authorize(Policy = Policies.ManagerAccess)]
    public async Task<ActionResult<UniversityDto>> Create([FromBody] CreateUniversityDto dto, CancellationToken cancellationToken)
    {
        var university = await _service.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = university.Id }, university);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.ManagerAccess)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUniversityDto dto, CancellationToken cancellationToken)
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