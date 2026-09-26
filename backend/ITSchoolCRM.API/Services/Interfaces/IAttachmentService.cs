using ITSchoolCRM.API.DTOs.Attachments;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IAttachmentService
{
    Task<AttachmentDto> UploadAsync(int interactionId,  int statusId, IFormFile file, CancellationToken cancellationToken);

    Task<List<AttachmentDto>> GetByInteractionAsync(int interactionId, CancellationToken cancellationToken);

    Task<AttachmentDto?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<(Stream Stream, string ContentType, string FileName)?> DownloadAsync(int id, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}