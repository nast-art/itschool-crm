using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Attachments;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using ITSchoolCRM.API.Storage;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Сервис вложений (функциональное требование 3 ТЗ:
/// png, jpeg, pdf, zip, gzip, rar, doc, docx, xls, xlsx).
///
/// КЭШИРОВАНИЕ (KeyDB, cache-aside):
///   • GetByInteractionAsync → ключ interaction:{id}:
///     attachments:v{interaction:version}. Доступ проверяется
///     ДО кэша (дешёвым запросом university_id + проверка по
///     university_managers), поэтому ключ без скопа: кэш не
///     отдаст список чужого вуза. Сам список — метаданные
///     из PostgreSQL; сами файлы кэшу не подлежат (их
///     отдаёт DownloadAsync потоком с диска/S3);
///   • UploadAsync/DeleteAsync → после SaveChangesAsync
///     INCR interaction:version: список вложений обновляется
///     у всех, кто смотрит карточку;
///   • GetByIdAsync / DownloadAsync → без кэша: одноразовые
///     операции (скачивание — поток с хранилища, кэшировать
///     бинарные данные в KeyDB нерационально).
///
/// ХРАНИЛИЩЕ: файловые операции через IFileStorage. Замена
/// реализации (диск → S3) не затрагивает этот сервис.
/// </summary>
public class AttachmentService : IAttachmentService
{
    private readonly CrmDbContext _context;
    private readonly IUserAccessService _accessService;
    private readonly IAuditService _auditService;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;
    private readonly IFileStorage _storage;

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
        ICacheService cache,
        CacheOptions cacheOptions,
        IFileStorage storage)
    {
        _context = context;
        _accessService = accessService;
        _auditService = auditService;
        _cache = cache;
        _cacheOptions = cacheOptions;
        _storage = storage;
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

        // Ключ хранилища: тот же формат, что раньше попадал
        // в storage_path — миграция на S3 не потребует
        // перезаписи существующих записей в БД.
        var storageKey =
            Path.Combine(
                    "uploads",
                    "attachments",
                    interactionId.ToString(),
                    statusId.ToString(),
                    uniqueFileName)
                .Replace(
                    '\\',
                    '/');

        // Сохраняем файл через абстракцию хранилища.
        // Если хранилище недоступно — исключение уйдёт в
        // глобальный обработчик (ERR_INTERNAL), запись в БД
        // НЕ появится: метаданные без файла — «осиротевшая»
        // запись, такого состояния не допускаем.
        await using (var input = file.OpenReadStream())
        {
            await _storage.SaveAsync(
                input,
                storageKey,
                AllowedExtensions[extension],
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
                    storageKey,

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

        // НОВОЕ (кэш): файл появился в списке вложений карточки.
        await _cache.BumpVersionAsync(
            CacheKeys.InteractionVersionKey,
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
        // Доступ ДО кэша: так кэш никогда не отдаст список
        // чужого вуза. Проверка — та же пара запросов, что и
        // в InteractionService (university_id + university_managers).
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

        // Ключ версионирован interaction:version: загрузка и
        // удаление файлов бампят тот же счётчик — список всегда
        // согласован с карточкой и историей.
        return await _cache.GetOrCreateAsync(
            CacheKeys.Attachments(interactionId),
            _cacheOptions.InteractionTtl,
            _ => LoadByInteractionFromDatabaseAsync(
                interactionId,
                cancellationToken),
            cancellationToken);
    }

    /// <summary>Исходная выборка вложений (бывшее тело GetByInteractionAsync).</summary>
    private async Task<List<AttachmentDto>>
        LoadByInteractionFromDatabaseAsync(
            int interactionId,
            CancellationToken cancellationToken)
    {
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
        // БЕЗ КЭША: одноразовые операции (открытие карточки
        // файла, ссылки). Кэшировать метаданные одного файла
        // под отдельным ключом невыгодно: обращений мало,
        // а пер-ключ инвалидация усложнила бы схему версий.
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
        // Метаданные — из БД (без кэша, см. GetByIdAsync),
        // поток — из хранилища. Кэшировать бинарные данные
        // в KeyDB нерационально: файлы до 50 МБ, повторные
        // скачивания одного файла одним пользователем редки,
        // а браузер и так кэширует скачанное по своим правилам.
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

        // Проверка path traversal теперь внутри хранилища
        // (GetSafeFullPath) — вызывающему коду она не видна.
        var stream =
            await _storage.OpenReadAsync(
                attachment.storage_path,
                cancellationToken);

        if (stream is null)
        {
            return null;
        }

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

        // Удаляем файл через абстракцию хранилища. Ошибка
        // хранилища НЕ отменяет удаление метаданных: иначе
        // «осиротевшая» запись в БД заблокировала бы очистку.
        // Реализация DeleteAsync сама глотает «файл не найден».
        if (!string.IsNullOrWhiteSpace(
                attachment.storage_path))
        {
            await _storage.DeleteAsync(
                attachment.storage_path,
                cancellationToken);
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

        // НОВОЕ (кэш): файл исчез из списка вложений карточки.
        await _cache.BumpVersionAsync(
            CacheKeys.InteractionVersionKey,
            cancellationToken);

        return true;
    }
}