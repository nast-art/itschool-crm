using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Attachments;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class AttachmentService : IAttachmentService
{
    private readonly CrmDbContext _context;
    private readonly IUserAccessService _accessService;
    private readonly IAuditService _auditService;
    private readonly IWebHostEnvironment _environment;

    private const long MaxFileSize =
        50L * 1024L * 1024L;

    private static readonly Dictionary<string, string>
        AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                [".png"] = "image/png",
                [".jpeg"] = "image/jpeg",
                [".jpg"] = "image/jpeg",
                [".pdf"] = "application/pdf",
                [".zip"] = "application/zip",
                [".gzip"] = "application/gzip",
                [".gz"] = "application/gzip",
                [".rar"] = "application/vnd.rar",
                [".doc"] = "application/msword",
                [".docx"] =
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                [".xls"] = "application/vnd.ms-excel",
                [".xlsx"] =
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

    public AttachmentService(
        CrmDbContext context,
        IUserAccessService accessService,
        IAuditService auditService,
        IWebHostEnvironment environment)
    {
        _context = context;
        _accessService = accessService;
        _auditService = auditService;
        _environment = environment;
    }

   public async Task<AttachmentDto> UploadAsync(
    int interactionId,
    int statusId,
    IFormFile file,
    CancellationToken cancellationToken)
{
    if (file is null ||
        file.Length <= 0)
    {
        throw new ArgumentException(
            "Файл не выбран.");
    }

    if (file.Length > MaxFileSize)
    {
        throw new InvalidOperationException(
            "Размер файла не должен превышать 50 МБ.");
    }

    var extension =
        Path.GetExtension(file.FileName);

    if (string.IsNullOrWhiteSpace(extension) ||
        !AllowedExtensions.ContainsKey(extension))
    {
        throw new InvalidOperationException(
            "Формат файла не поддерживается.");
    }

    var interaction =
        await _context.interactions
            .FirstOrDefaultAsync(
                x =>
                    x.interactions_id ==
                    interactionId,
                cancellationToken);

    if (interaction is null)
    {
        throw new KeyNotFoundException(
            "Interaction не найден.");
    }

    if (!interaction.university_id.HasValue)
    {
        throw new InvalidOperationException(
            "У interaction отсутствует ВУЗ.");
    }

    var hasAccess =
        await _accessService
            .HasAccessToUniversityAsync(
                interaction.university_id.Value,
                cancellationToken);

    if (!hasAccess)
    {
        throw new UnauthorizedAccessException(
            "Нет доступа к interaction.");
    }

    var status =
        await _context.workflow_statuses
            .FirstOrDefaultAsync(
                x =>
                    x.workflow_statuses_id ==
                    statusId,
                cancellationToken);

    if (status is null)
    {
        throw new KeyNotFoundException(
            "Статус не найден.");
    }

    if (interaction.workflow_id !=
        status.workflow_id)
    {
        throw new InvalidOperationException(
            "Статус не принадлежит workflow interaction.");
    }

    var safeFileName =
        Path.GetFileName(
            file.FileName);

    var uniqueFileName =
        $"{Guid.NewGuid():N}_{safeFileName}";

    var relativeDirectory =
        Path.Combine(
            "uploads",
            "attachments",
            interactionId.ToString(),
            statusId.ToString());

    var absoluteDirectory =
        Path.Combine(
            _environment.ContentRootPath,
            relativeDirectory);

    Directory.CreateDirectory(
        absoluteDirectory);

    var absolutePath =
        Path.Combine(
            absoluteDirectory,
            uniqueFileName);

    await using (
        var stream =
            new FileStream(
                absolutePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true))
    {
        await file.CopyToAsync(
            stream,
            cancellationToken);
    }

    var databaseUserId =
        await _accessService
            .GetCurrentDatabaseUserIdAsync(
                cancellationToken);

    var attachment =
        new attachment
        {
            interaction_id =
                interactionId,

            status_id =
                statusId,

            uploaded_by =
                databaseUserId,

            file_name =
                safeFileName,

            storage_path =
                Path.Combine(
                    relativeDirectory,
                    uniqueFileName)
                .Replace(
                    '\\',
                    '/'),

            mime_type =
                AllowedExtensions[
                    extension],

            file_size =
                file.Length,

            created_at =
                DateTime.UtcNow
        };

    _context.attachments.Add(
        attachment);

    await _context.SaveChangesAsync(
        cancellationToken);

    await _auditService.WriteAsync(
        "UPLOAD",
        "attachment",
        attachment.attachments_id,
        null,
        new
        {
            attachment.attachments_id,
            attachment.interaction_id,
            attachment.status_id,
            attachment.uploaded_by,
            attachment.file_name,
            attachment.mime_type,
            attachment.file_size
        },
        cancellationToken);

    return new AttachmentDto
    {
        Id =
            attachment.attachments_id,

        InteractionId =
            attachment.interaction_id,

        StatusId =
            attachment.status_id,

        // Название этапа уже загружено в status выше — дешевле, чем повторный запрос
        StatusName =
            status.name,

        UploadedBy =
            attachment.uploaded_by,

        FileName =
            attachment.file_name,

        MimeType =
            attachment.mime_type,

        FileSize =
            attachment.file_size,

        CreatedAt =
            attachment.created_at
    };
}
    public async Task<List<AttachmentDto>>
    GetByInteractionAsync(
        int interactionId,
        CancellationToken cancellationToken)
{
    var interaction =
        await _context.interactions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x =>
                    x.interactions_id ==
                    interactionId,
                cancellationToken);

    if (interaction is null)
    {
        return new List<AttachmentDto>();
    }

    if (!interaction.university_id.HasValue)
    {
        return new List<AttachmentDto>();
    }

    var hasAccess =
        await _accessService
            .HasAccessToUniversityAsync(
                interaction.university_id.Value,
                cancellationToken);

    if (!hasAccess)
    {
        return new List<AttachmentDto>();
    }

    return await _context.attachments
        .AsNoTracking()
        .Where(x =>
            x.interaction_id ==
            interactionId)
        .OrderByDescending(
            x => x.created_at)
        .Select(x => new AttachmentDto
        {
            Id =
                x.attachments_id,

            InteractionId =
                x.interaction_id,

            StatusId =
                x.status_id,

            StatusName =
                x.status != null
                    ? x.status.name
                    : null,

            UploadedBy =
                x.uploaded_by,

            FileName =
                x.file_name,

            MimeType =
                x.mime_type,

            FileSize =
                x.file_size,

            CreatedAt =
                x.created_at
        })
        .ToListAsync(
            cancellationToken);
}

   public async Task<AttachmentDto?> GetByIdAsync(
    int id,
    CancellationToken cancellationToken)
{
    var attachment =
        await _context.attachments
            .AsNoTracking()
            .Include(x =>
                x.interaction)
            .FirstOrDefaultAsync(
                x =>
                    x.attachments_id == id,
                cancellationToken);

    if (attachment is null)
    {
        return null;
    }

    if (!attachment.interaction?
        .university_id.HasValue == true)
    {
        return null;
    }

    var hasAccess =
        await _accessService
            .HasAccessToUniversityAsync(
                attachment.interaction!
                    .university_id!.Value,
                cancellationToken);

    if (!hasAccess)
    {
        return null;
    }

    // Имя этапа для привязки (навигация status у attachment)
    var statusName =
        await _context.workflow_statuses
            .AsNoTracking()
            .Where(x =>
                x.workflow_statuses_id ==
                attachment.status_id)
            .Select(x => x.name)
            .FirstOrDefaultAsync(
                cancellationToken);

    return new AttachmentDto
    {
        Id =
            attachment.attachments_id,

        InteractionId =
            attachment.interaction_id,

        StatusId =
            attachment.status_id,

        StatusName =
            statusName,

        UploadedBy =
            attachment.uploaded_by,

        FileName =
            attachment.file_name,

        MimeType =
            attachment.mime_type,

        FileSize =
            attachment.file_size,

        CreatedAt =
            attachment.created_at
    };
}

    public async Task<(
        Stream Stream,
        string ContentType,
        string FileName)?> DownloadAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var attachment =
            await _context.attachments
                .AsNoTracking()
                .Include(x =>
                    x.interaction)
                .FirstOrDefaultAsync(
                    x =>
                        x.attachments_id == id,
                    cancellationToken);

        if (attachment is null)
        {
            return null;
        }

        if (!attachment.interaction?
            .university_id.HasValue == true)
        {
            return null;
        }

        var hasAccess =
            await _accessService
                .HasAccessToUniversityAsync(
                    attachment.interaction!
                        .university_id!.Value,
                    cancellationToken);

        if (!hasAccess)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(
                attachment.storage_path))
        {
            return null;
        }

        var root =
            Path.GetFullPath(
                Path.Combine(
                    _environment.ContentRootPath,
                    "uploads",
                    "attachments"));

        var fullPath =
            Path.GetFullPath(
                Path.Combine(
                    _environment.ContentRootPath,
                    attachment.storage_path));

        if (!fullPath.StartsWith(
                root,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Некорректный путь к файлу.");
        }

        if (!File.Exists(fullPath))
        {
            return null;
        }

        var stream =
            new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81920,
                useAsync: true);

        return (
            stream,
            attachment.mime_type ??
                "application/octet-stream",
            attachment.file_name ??
                "download");
    }

   public async Task<bool> DeleteAsync(
    int id,
    CancellationToken cancellationToken)
{
    var attachment =
        await _context.attachments
            .Include(x =>
                x.interaction)
            .FirstOrDefaultAsync(
                x =>
                    x.attachments_id == id,
                cancellationToken);

    if (attachment is null)
    {
        return false;
    }

    if (!attachment.interaction?
        .university_id.HasValue == true)
    {
        return false;
    }

    var databaseUserId =
        await _accessService
            .GetCurrentDatabaseUserIdAsync(
                cancellationToken);

    // Владелец (кто загрузил) удаляет свой файл без дополнительных
    // проверок; остальные — по доступу к вузу
    // (это покрывает руководителя и администратора)
    var isOwner =
        attachment.uploaded_by ==
        databaseUserId;

    if (!isOwner)
    {
        var universityId =
            attachment.interaction!
                .university_id!.Value;

        var hasAccess =
            await _accessService
                .HasAccessToUniversityAsync(
                    universityId,
                    cancellationToken);

        if (!hasAccess)
        {
            throw new UnauthorizedAccessException(
                "Нет доступа к attachment.");
        }
    }

    var oldData = new
    {
        attachment.attachments_id,
        attachment.interaction_id,
        attachment.status_id,
        attachment.uploaded_by,
        attachment.file_name,
        attachment.storage_path,
        attachment.mime_type,
        attachment.file_size
    };

    if (!string.IsNullOrWhiteSpace(
            attachment.storage_path))
    {
        var fullPath =
            Path.GetFullPath(
                Path.Combine(
                    _environment.ContentRootPath,
                    attachment.storage_path));

        var root =
            Path.GetFullPath(
                Path.Combine(
                    _environment.ContentRootPath,
                    "uploads",
                    "attachments"));

        if (fullPath.StartsWith(
                root,
                StringComparison.OrdinalIgnoreCase)
            &&
            File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    _context.attachments.Remove(
        attachment);

    await _context.SaveChangesAsync(
        cancellationToken);

    await _auditService.WriteAsync(
        "DELETE",
        "attachment",
        id,
        oldData,
        null,
        cancellationToken);

    return true;
}
}