using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Directions;
using ITSchoolCRM.API.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DirectionsController : ControllerBase
{
    private readonly IDirectionService _service;

    public DirectionsController(
        IDirectionService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<DirectionDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var directions =
            await _service.GetAllAsync(
                cancellationToken);

        return Ok(directions);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<DirectionDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var direction =
            await _service.GetByIdAsync(
                id,
                cancellationToken);

        if (direction is null)
        {
            return NotFound();
        }

        return Ok(direction);
    }

    [HttpGet("active")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<DirectionDto>>> GetActive(
        CancellationToken cancellationToken)
    {
        var directions =
            await _service.GetActiveAsync(
                cancellationToken);

        return Ok(directions);
    }

    [HttpGet("search")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<DirectionDto>>> Search(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var directions =
            await _service.SearchAsync(
                search,
                cancellationToken);

        return Ok(directions);
    }

    [HttpPost]
    [Authorize(Policy = Policies.ManagerAccess)]
    public async Task<ActionResult<DirectionDto>> Create(
        [FromBody] CreateDirectionDto dto,
        CancellationToken cancellationToken)
    {
        var direction =
            await _service.CreateAsync(
                dto,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = direction.Id },
            direction);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.ManagerAccess)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDirectionDto dto,
        CancellationToken cancellationToken)
    {
        var result =
            await _service.UpdateAsync(
                id,
                dto,
                cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var result =
            await _service.DeleteAsync(
                id,
                cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}