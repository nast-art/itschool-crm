using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Attachments;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentService _service;

    public AttachmentsController(IAttachmentService service)
    {
        _service = service;
    }

    [HttpPost("interaction/{interactionId:int}/status/{statusId:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    [RequestSizeLimit(50L * 1024L * 1024L)]
    public async Task<ActionResult<AttachmentDto>> Upload(int interactionId, int statusId, IFormFile file, CancellationToken cancellationToken)
    {
        var attachment =await _service.UploadAsync(interactionId, statusId, file, cancellationToken);
        return Ok(attachment);
    }

    [HttpGet("interaction/{interactionId:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<
        ActionResult<List<AttachmentDto>>> GetByInteraction(int interactionId, CancellationToken cancellationToken)
    {
        var attachments = await _service.GetByInteractionAsync(interactionId, cancellationToken);
        return Ok(attachments);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<AttachmentDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var attachment = await _service.GetByIdAsync(id, cancellationToken);

        if (attachment is null)
        {
            return NotFound();
        }

        return Ok(attachment);
    }

    [HttpGet("{id:int}/download")]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var result = await _service.DownloadAsync(id, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

         var contentDisposition = new ContentDispositionHeaderValue("attachment")
    {
        FileName = "attachment",
        FileNameStar = result.Value.FileName,
    };

    Response.Headers.ContentDisposition = contentDisposition.ToString();

    return File(result.Value.Stream, result.Value.ContentType, enableRangeProcessing: true);
    }

    // Удаление: владелец файла или руководитель/админ
    // (проверка прав — в сервисе)
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.UserAccess)]
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