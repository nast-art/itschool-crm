using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.WorkflowTransitions;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowTransitionsController : ControllerBase
{
    private readonly IWorkflowTransitionService _service;

    public WorkflowTransitionsController(
        IWorkflowTransitionService service)
    {
        _service = service;
    }

    [HttpGet("workflow/{workflowId:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<WorkflowTransitionDto>>> GetByWorkflow(
        int workflowId,
        CancellationToken cancellationToken)
    {
        var transitions =
            await _service.GetByWorkflowIdAsync(
                workflowId,
                cancellationToken);

        return Ok(transitions);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<WorkflowTransitionDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var transition =
            await _service.GetByIdAsync(
                id,
                cancellationToken);

        if (transition is null)
        {
            return NotFound();
        }

        return Ok(transition);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<ActionResult<WorkflowTransitionDto>> Create(
        [FromBody] CreateWorkflowTransitionDto dto,
        CancellationToken cancellationToken)
    {
        var transition =
            await _service.CreateAsync(
                dto,
                cancellationToken);

        if (transition is null)
        {
            return NotFound();
        }

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = transition.Id
            },
            transition);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateWorkflowTransitionDto dto,
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