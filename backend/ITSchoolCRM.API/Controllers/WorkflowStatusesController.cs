using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.WorkflowStatuses;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkflowStatusesController : ControllerBase
{
    private readonly IWorkflowStatusService _service;

    public WorkflowStatusesController(
        IWorkflowStatusService service)
    {
        _service = service;
    }

    [HttpGet("workflow/{workflowId:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<WorkflowStatusDto>>> GetByWorkflow(
        int workflowId,
        CancellationToken cancellationToken)
    {
        var statuses =
            await _service.GetByWorkflowIdAsync(
                workflowId,
                cancellationToken);

        return Ok(statuses);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<WorkflowStatusDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var status =
            await _service.GetByIdAsync(
                id,
                cancellationToken);

        if (status is null)
        {
            return NotFound();
        }

        return Ok(status);
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<ActionResult<WorkflowStatusDto>> Create(
        [FromBody] CreateWorkflowStatusDto dto,
        CancellationToken cancellationToken)
    {
        var status =
            await _service.CreateAsync(
                dto,
                cancellationToken);

        if (status is null)
        {
            return NotFound();
        }

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = status.Id
            },
            status);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateWorkflowStatusDto dto,
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