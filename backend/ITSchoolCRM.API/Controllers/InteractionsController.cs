using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Interactions;
using ITSchoolCRM.API.DTOs.InteractionHistory;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InteractionsController : ControllerBase
{
    private readonly IInteractionService _service;

    public InteractionsController(
        IInteractionService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<InteractionDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var interactions =
            await _service.GetAllAsync(
                cancellationToken);

        return Ok(interactions);
    }

    [HttpGet("university/{universityId:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<InteractionDto>>> GetByUniversity(
        int universityId,
        CancellationToken cancellationToken)
    {
        var interactions =
            await _service.GetByUniversityIdAsync(
                universityId,
                cancellationToken);

        return Ok(interactions);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<InteractionDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var interaction =
            await _service.GetByIdAsync(
                id,
                cancellationToken);

        if (interaction is null)
        {
            return NotFound();
        }

        return Ok(interaction);
    }

    [HttpPost]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<InteractionDto>> Create(
        [FromBody] CreateInteractionDto dto,
        CancellationToken cancellationToken)
    {
        var interaction =
            await _service.CreateAsync(
                dto,
                cancellationToken);

        if (interaction is null)
        {
            return Forbid();
        }

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = interaction.Id
            },
            interaction);
    }

    // Редактирование карточки (кнопка «Редактировать» у admin на фронте)
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<InteractionDto>> Update(
        int id,
        [FromBody] UpdateInteractionDto dto,
        CancellationToken cancellationToken)
    {
        var interaction =
            await _service.UpdateAsync(
                id,
                dto,
                cancellationToken);

        if (interaction is null)
        {
            return NotFound();
        }

        return Ok(interaction);
    }

    [HttpPost("{id:int}/status")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<IActionResult> ChangeStatus(
        int id,
        [FromBody] ChangeInteractionStatusDto dto,
        CancellationToken cancellationToken)
    {
        var result =
            await _service.ChangeStatusAsync(
                id,
                dto,
                cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    // Комментарий без смены статуса
    [HttpPost("{id:int}/comments")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<IActionResult> AddComment(
        int id,
        [FromBody] AddInteractionCommentDto dto,
        CancellationToken cancellationToken)
    {
        var result =
            await _service.AddCommentAsync(
                id,
                dto,
                cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("{id:int}/history")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<InteractionStatusHistoryDto>>> GetHistory(
        int id,
        CancellationToken cancellationToken)
    {
        var history =
            await _service.GetHistoryAsync(
                id,
                cancellationToken);

        return Ok(history);
    }
}