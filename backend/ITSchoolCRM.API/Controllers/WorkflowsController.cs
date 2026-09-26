using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Workflows;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowsController : ControllerBase
{
    private readonly IWorkflowService _service;

    public WorkflowsController(IWorkflowService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<WorkflowDto>>> GetAll(CancellationToken cancellationToken)
    {
        var workflows = await _service.GetAllAsync(cancellationToken);

        return Ok(workflows);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<WorkflowDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var workflow = await _service.GetByIdAsync(id, cancellationToken);

        if (workflow is null)
        {
            return NotFound();
        }

        return Ok(workflow);
    }

    [HttpGet("active")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<WorkflowDto>>> GetActive(CancellationToken cancellationToken)
    {
        var workflows = await _service.GetActiveAsync(cancellationToken);

        return Ok(workflows);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<ActionResult<WorkflowDto>> Create([FromBody] CreateWorkflowDto dto, CancellationToken cancellationToken)
    {
        var workflow = await _service.CreateAsync(dto, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = workflow.Id }, workflow);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkflowDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}