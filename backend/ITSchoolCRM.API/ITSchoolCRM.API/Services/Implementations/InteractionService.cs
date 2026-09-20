using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Interactions;
using ITSchoolCRM.API.DTOs.InteractionHistory;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class InteractionService : IInteractionService
{
    private readonly CrmDbContext _context;
    private readonly IUserAccessService _accessService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public InteractionService(
        CrmDbContext context,
        IUserAccessService accessService,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _context = context;
        _accessService = accessService;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<List<InteractionDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var accessibleUniversityIds =
            _accessService.GetAccessibleUniversityIds();

        return await _context.interactions
            .AsNoTracking()
            .Where(x =>
                x.university_id.HasValue &&
                accessibleUniversityIds.Contains(
                    x.university_id.Value))
            .OrderByDescending(x => x.updated_at)
            .Select(x => new InteractionDto
            {
                Id = x.interactions_id,

                UniversityId = x.university_id,

                UniversityName =
                    x.university != null
                        ? x.university.name
                        : null,

                ProgramId = x.program_id,

                ProgramName =
                    x.program != null
                        ? x.program.name
                        : null,

                ProductId = x.product_id,

                ProductName =
                    x.product != null
                        ? x.product.name
                        : null,

                ManagerId = x.manager_id,

                // Колонки full_name нет: ФИО собирается из частей.
                ManagerName =
                    x.manager == null
                        ? null
                        : x.manager.middle_name == null
                            ? x.manager.last_name + " " + x.manager.first_name
                            : x.manager.last_name + " " + x.manager.first_name + " " + x.manager.middle_name,

                UniversityContactId =
                    x.university_contact_id,

                UniversityContactName =
                    x.university_contact != null
                        ? x.university_contact.full_name
                        : null,

                WorkflowId = x.workflow_id,

                WorkflowName =
                    x.workflow != null
                        ? x.workflow.name
                        : null,

                CurrentStatusId =
                    x.current_status_id,

                CurrentStatusName =
                    x.current_status != null
                        ? x.current_status.name
                        : null,

                ContractId = x.contract_id,

                LicenseId = x.license_id,

                CreatedAt = x.created_at,

                UpdatedAt = x.updated_at
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<InteractionDto>> GetByUniversityIdAsync(
        int universityId,
        CancellationToken cancellationToken)
    {
        var hasAccess =
            await _accessService
                .HasAccessToUniversityAsync(
                    universityId,
                    cancellationToken);

        if (!hasAccess)
        {
            return new List<InteractionDto>();
        }

        return await _context.interactions
            .AsNoTracking()
            .Where(x =>
                x.university_id ==
                universityId)
            .OrderByDescending(x => x.updated_at)
            .Select(x => new InteractionDto
            {
                Id = x.interactions_id,

                UniversityId = x.university_id,

                UniversityName =
                    x.university != null
                        ? x.university.name
                        : null,

                ProgramId = x.program_id,

                ProgramName =
                    x.program != null
                        ? x.program.name
                        : null,

                ProductId = x.product_id,

                ProductName =
                    x.product != null
                        ? x.product.name
                        : null,

                ManagerId = x.manager_id,

                // Колонки full_name нет: ФИО собирается из частей.
                ManagerName =
                    x.manager == null
                        ? null
                        : x.manager.middle_name == null
                            ? x.manager.last_name + " " + x.manager.first_name
                            : x.manager.last_name + " " + x.manager.first_name + " " + x.manager.middle_name,

                UniversityContactId =
                    x.university_contact_id,

                UniversityContactName =
                    x.university_contact != null
                        ? x.university_contact.full_name
                        : null,

                WorkflowId = x.workflow_id,

                WorkflowName =
                    x.workflow != null
                        ? x.workflow.name
                        : null,

                CurrentStatusId =
                    x.current_status_id,

                CurrentStatusName =
                    x.current_status != null
                        ? x.current_status.name
                        : null,

                ContractId = x.contract_id,

                LicenseId = x.license_id,

                CreatedAt = x.created_at,

                UpdatedAt = x.updated_at
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<InteractionDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var interaction =
            await _context.interactions
                .AsNoTracking()
                .Where(x =>
                    x.interactions_id == id)
                .Select(x => new
                {
                    Interaction = x,

                    UniversityId =
                        x.university_id
                })
                .FirstOrDefaultAsync(
                    cancellationToken);

        if (interaction is null)
        {
            return null;
        }

        if (!interaction.UniversityId.HasValue)
        {
            return null;
        }

        var hasAccess =
            await _accessService
                .HasAccessToUniversityAsync(
                    interaction.UniversityId.Value,
                    cancellationToken);

        if (!hasAccess)
        {
            return null;
        }

        return await _context.interactions
            .AsNoTracking()
            .Where(x =>
                x.interactions_id == id)
            .Select(x => new InteractionDto
            {
                Id = x.interactions_id,

                UniversityId = x.university_id,

                UniversityName =
                    x.university != null
                        ? x.university.name
                        : null,

                ProgramId = x.program_id,

                ProgramName =
                    x.program != null
                        ? x.program.name
                        : null,

                ProductId = x.product_id,

                ProductName =
                    x.product != null
                        ? x.product.name
                        : null,

                ManagerId = x.manager_id,

                // Колонки full_name нет: ФИО собирается из частей.
                ManagerName =
                    x.manager == null
                        ? null
                        : x.manager.middle_name == null
                            ? x.manager.last_name + " " + x.manager.first_name
                            : x.manager.last_name + " " + x.manager.first_name + " " + x.manager.middle_name,

                UniversityContactId =
                    x.university_contact_id,

                UniversityContactName =
                    x.university_contact != null
                        ? x.university_contact.full_name
                        : null,

                WorkflowId = x.workflow_id,

                WorkflowName =
                    x.workflow != null
                        ? x.workflow.name
                        : null,

                CurrentStatusId =
                    x.current_status_id,

                CurrentStatusName =
                    x.current_status != null
                        ? x.current_status.name
                        : null,

                ContractId = x.contract_id,

                LicenseId = x.license_id,

                CreatedAt = x.created_at,

                UpdatedAt = x.updated_at
            })
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public async Task<InteractionDto?> CreateAsync(
        CreateInteractionDto dto,
        CancellationToken cancellationToken)
    {
        if (!dto.UniversityId.HasValue)
        {
            throw new ArgumentException(
                "UniversityId обязателен.");
        }

        if (!dto.WorkflowId.HasValue)
        {
            throw new ArgumentException(
                "WorkflowId обязателен.");
        }

        var hasUniversityAccess =
            await _accessService
                .HasAccessToUniversityAsync(
                    dto.UniversityId.Value,
                    cancellationToken);

        if (!hasUniversityAccess)
        {
            return null;
        }

        var universityExists =
            await _context.universities
                .AnyAsync(
                    x =>
                        x.universities_id ==
                        dto.UniversityId.Value &&
                        x.is_active == true,
                    cancellationToken);

        if (!universityExists)
        {
            throw new InvalidOperationException(
                "Указанный ВУЗ не найден или неактивен.");
        }

        if (dto.ProgramId.HasValue)
        {
            var programExists =
                await _context.it_programs
                    .AnyAsync(
                        x =>
                            x.it_programs_id ==
                            dto.ProgramId.Value &&
                            x.is_active == true,
                        cancellationToken);

            if (!programExists)
            {
                throw new InvalidOperationException(
                    "Указанная ИТ-программа не найдена или неактивна.");
            }
        }

        if (dto.ProductId.HasValue)
        {
            var productExists =
                await _context.it_products
                    .AnyAsync(
                        x =>
                            x.it_products_id ==
                            dto.ProductId.Value &&
                            x.is_active == true,
                        cancellationToken);

            if (!productExists)
            {
                throw new InvalidOperationException(
                    "Указанный ИТ-продукт не найден или неактивен.");
            }
        }

        var workflow =
            await _context.workflows
                .FirstOrDefaultAsync(
                    x =>
                        x.workflows_id ==
                            dto.WorkflowId.Value &&
                        x.is_active == true,
                    cancellationToken);

        if (workflow is null)
        {
            throw new InvalidOperationException(
                "Workflow не найден или неактивен.");
        }

        var initialStatus =
            await _context.workflow_statuses
                .Where(x =>
                    x.workflow_id ==
                        dto.WorkflowId.Value &&
                    x.is_initial == true)
                .OrderBy(x => x.sort_order)
                .FirstOrDefaultAsync(
                    cancellationToken);

        if (initialStatus is null)
        {
            throw new InvalidOperationException(
                "Для workflow не задан начальный статус.");
        }

        if (dto.ManagerId.HasValue)
        {
            var managerExists =
                await _context.users
                    .AnyAsync(
                        x =>
                            x.users_id ==
                                dto.ManagerId.Value &&
                            x.is_active == true,
                        cancellationToken);

            if (!managerExists)
            {
                throw new InvalidOperationException(
                    "Ответственный пользователь не найден или неактивен.");
            }
        }

        if (dto.UniversityContactId.HasValue)
        {
            var contactExists =
                await _context.university_contacts
                    .AnyAsync(
                        x =>
                            x.university_contacts_id ==
                                dto.UniversityContactId.Value &&
                            x.university_id ==
                                dto.UniversityId.Value &&
                            x.is_active == true,
                        cancellationToken);

            if (!contactExists)
            {
                throw new InvalidOperationException(
                    "Ответственный от ВУЗа не найден или не относится к указанному ВУЗу.");
            }
        }

        var interaction = new interaction
        {
            university_id = dto.UniversityId,
            program_id = dto.ProgramId,
            product_id = dto.ProductId,
            manager_id = dto.ManagerId,
            university_contact_id =
                dto.UniversityContactId,
            workflow_id = dto.WorkflowId,
            current_status_id =
                initialStatus.workflow_statuses_id,
            contract_id = dto.ContractId,
            license_id = dto.LicenseId,
            created_at = DateTime.UtcNow,
            updated_at = DateTime.UtcNow
        };

        _context.interactions.Add(
            interaction);

        await _context.SaveChangesAsync(
            cancellationToken);

        await _auditService.WriteAsync(
            "CREATE",
            "interaction",
            interaction.interactions_id,
            null,
            new
            {
                interaction.interactions_id,
                interaction.university_id,
                interaction.program_id,
                interaction.product_id,
                interaction.manager_id,
                interaction.university_contact_id,
                interaction.workflow_id,
                interaction.current_status_id,
                interaction.contract_id,
                interaction.license_id
            },
            cancellationToken);

        return await GetByIdAsync(
            interaction.interactions_id,
            cancellationToken);
    }

    public async Task<bool> ChangeStatusAsync(
        int interactionId,
        ChangeInteractionStatusDto dto,
        CancellationToken cancellationToken)
    {
        if (!dto.ToStatusId.HasValue)
        {
            throw new ArgumentException(
                "ToStatusId обязателен.");
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
            return false;
        }

        if (!interaction.university_id.HasValue)
        {
            return false;
        }

        var hasAccess =
            await _accessService
                .HasAccessToUniversityAsync(
                    interaction.university_id.Value,
                    cancellationToken);

        if (!hasAccess)
        {
            return false;
        }

        if (!interaction.workflow_id.HasValue ||
            !interaction.current_status_id.HasValue)
        {
            throw new InvalidOperationException(
                "У interaction отсутствует workflow или текущий статус.");
        }

        var targetStatus =
            await _context.workflow_statuses
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_statuses_id ==
                        dto.ToStatusId.Value,
                    cancellationToken);

        if (targetStatus is null)
        {
            throw new InvalidOperationException(
                "Целевой статус не найден.");
        }

        if (targetStatus.workflow_id !=
            interaction.workflow_id)
        {
            throw new InvalidOperationException(
                "Целевой статус принадлежит другому workflow.");
        }

        if (interaction.current_status_id ==
            dto.ToStatusId)
        {
            throw new InvalidOperationException(
                "Interaction уже находится в указанном статусе.");
        }

        var transition =
            await _context.workflow_transitions
                .FirstOrDefaultAsync(
                    x =>
                        x.workflow_id ==
                            interaction.workflow_id &&
                        x.from_status_id ==
                            interaction.current_status_id &&
                        x.to_status_id ==
                            dto.ToStatusId.Value,
                    cancellationToken);

        if (transition is null)
        {
            throw new InvalidOperationException(
                "Переход между указанными статусами запрещён текущим workflow.");
        }

        var oldStatusId =
            interaction.current_status_id;

        var oldStatusName =
            await _context.workflow_statuses
                .Where(x =>
                    x.workflow_statuses_id ==
                    oldStatusId.Value)
                .Select(x => x.name)
                .FirstOrDefaultAsync(
                    cancellationToken);

        interaction.current_status_id =
            dto.ToStatusId;

        interaction.updated_at =
            DateTime.UtcNow;

        var databaseUserId =
            await _accessService
                .GetCurrentDatabaseUserIdAsync(
                    cancellationToken);

        var history =
            new interaction_status_history
            {
                interaction_id =
                    interaction.interactions_id,

                from_status_id =
                    oldStatusId,

                to_status_id =
                    dto.ToStatusId,

                changed_by =
                    databaseUserId,

                comment =
                    dto.Comment,

                changed_at =
                    DateTime.UtcNow
            };

        _context.interaction_status_histories.Add(
            history);

        await _context.SaveChangesAsync(
            cancellationToken);

        await _auditService.WriteAsync(
            "STATUS_CHANGE",
            "interaction",
            interaction.interactions_id,
            new
            {
                StatusId = oldStatusId,
                StatusName = oldStatusName
            },
            new
            {
                StatusId =
                    targetStatus.workflow_statuses_id,
                StatusName =
                    targetStatus.name,
                Comment =
                    dto.Comment
            },
            cancellationToken);

        return true;
    }

    public async Task<List<InteractionStatusHistoryDto>> GetHistoryAsync(
        int interactionId,
        CancellationToken cancellationToken)
    {
        var interaction =
            await _context.interactions
                .AsNoTracking()
                .Where(x =>
                    x.interactions_id ==
                    interactionId)
                .Select(x => new
                {
                    x.interactions_id,
                    x.university_id
                })
                .FirstOrDefaultAsync(
                    cancellationToken);

        if (interaction is null ||
            !interaction.university_id.HasValue)
        {
            return new List<InteractionStatusHistoryDto>();
        }

        var hasAccess =
            await _accessService
                .HasAccessToUniversityAsync(
                    interaction.university_id.Value,
                    cancellationToken);

        if (!hasAccess)
        {
            return new List<InteractionStatusHistoryDto>();
        }

        return await _context.interaction_status_histories
            .AsNoTracking()
            .Where(x =>
                x.interaction_id ==
                interactionId)
            .OrderByDescending(x => x.changed_at)
            .Select(x => new InteractionStatusHistoryDto
            {
                Id =
                    x.interaction_status_history_id,

                InteractionId =
                    x.interaction_id,

                FromStatusId =
                    x.from_status_id,

                FromStatusName =
                    x.from_status != null
                        ? x.from_status.name
                        : null,

                ToStatusId =
                    x.to_status_id,

                ToStatusName =
                    x.to_status != null
                        ? x.to_status.name
                        : null,

                ChangedBy =
                    x.changed_by,

                // Колонки full_name нет: ФИО собирается из частей.
                ChangedByName =
                    x.changed_byNavigation == null
                        ? null
                        : x.changed_byNavigation.middle_name == null
                            ? x.changed_byNavigation.last_name + " " + x.changed_byNavigation.first_name
                            : x.changed_byNavigation.last_name + " " + x.changed_byNavigation.first_name + " " + x.changed_byNavigation.middle_name,

                Comment =
                    x.comment,

                ChangedAt =
                    x.changed_at
            })
            .ToListAsync(
                cancellationToken);
    }

    public async Task<InteractionDto?> UpdateAsync(
    int id,
    UpdateInteractionDto dto,
    CancellationToken cancellationToken)
{
    var interaction =
        await _context.interactions
            .FirstOrDefaultAsync(
                x =>
                    x.interactions_id == id,
                cancellationToken);

    if (interaction is null ||
        !interaction.university_id.HasValue)
    {
        return null;
    }

    // Доступ к ТЕКУЩЕМУ вузу
    var hasAccess =
        await _accessService
            .HasAccessToUniversityAsync(
                interaction.university_id.Value,
                cancellationToken);

    if (!hasAccess)
    {
        return null;
    }

    if (!dto.UniversityId.HasValue)
    {
        throw new ArgumentException(
            "UniversityId обязателен.");
    }

    // Если вуз меняется — проверяем доступ и к НОВОМУ вузу
    if (dto.UniversityId.Value !=
        interaction.university_id.Value)
    {
        var hasNewAccess =
            await _accessService
                .HasAccessToUniversityAsync(
                    dto.UniversityId.Value,
                    cancellationToken);

        if (!hasNewAccess)
        {
            return null;
        }
    }

    var oldSnapshot = new
    {
        interaction.university_id,
        interaction.program_id,
        interaction.product_id,
        interaction.manager_id,
        interaction.university_contact_id
    };

    if (dto.ProgramId.HasValue)
    {
        var programExists =
            await _context.it_programs
                .AnyAsync(
                    x =>
                        x.it_programs_id ==
                            dto.ProgramId.Value &&
                        x.is_active == true,
                    cancellationToken);

        if (!programExists)
        {
            throw new InvalidOperationException(
                "Указанная ИТ-программа не найдена или неактивна.");
        }
    }

    if (dto.ProductId.HasValue)
    {
        var productExists =
            await _context.it_products
                .AnyAsync(
                    x =>
                        x.it_products_id ==
                            dto.ProductId.Value &&
                        x.is_active == true,
                    cancellationToken);

        if (!productExists)
        {
            throw new InvalidOperationException(
                "Указанный ИТ-продукт не найден или неактивен.");
        }
    }

    if (dto.ManagerId.HasValue)
    {
        var managerExists =
            await _context.users
                .AnyAsync(
                    x =>
                        x.users_id ==
                            dto.ManagerId.Value &&
                        x.is_active == true,
                    cancellationToken);

        if (!managerExists)
        {
            throw new InvalidOperationException(
                "Ответственный пользователь не найден или неактивен.");
        }
    }

    if (dto.UniversityContactId.HasValue)
    {
        var contactExists =
            await _context.university_contacts
                .AnyAsync(
                    x =>
                        x.university_contacts_id ==
                            dto.UniversityContactId.Value &&
                        x.university_id ==
                            dto.UniversityId.Value &&
                        x.is_active == true,
                    cancellationToken);

        if (!contactExists)
        {
            throw new InvalidOperationException(
                "Ответственный от ВУЗа не найден или не относится к указанному ВУЗу.");
        }
    }

    interaction.university_id = dto.UniversityId;
    interaction.program_id = dto.ProgramId;
    interaction.product_id = dto.ProductId;
    interaction.manager_id = dto.ManagerId;
    interaction.university_contact_id = dto.UniversityContactId;
    interaction.updated_at = DateTime.UtcNow;

    await _context.SaveChangesAsync(cancellationToken);

    await _auditService.WriteAsync(
        "UPDATE",
        "interaction",
        interaction.interactions_id,
        oldSnapshot,
        new
        {
            interaction.university_id,
            interaction.program_id,
            interaction.product_id,
            interaction.manager_id,
            interaction.university_contact_id
        },
        cancellationToken);

    return await GetByIdAsync(
        interaction.interactions_id,
        cancellationToken);
}

public async Task<bool> AddCommentAsync(
    int interactionId,
    AddInteractionCommentDto dto,
    CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(dto.Comment))
    {
        throw new ArgumentException(
            "Комментарий не может быть пустым.");
    }

    var interaction =
        await _context.interactions
            .Where(x =>
                x.interactions_id == interactionId)
            .Select(x => new
            {
                x.interactions_id,
                x.university_id,
                x.current_status_id
            })
            .FirstOrDefaultAsync(cancellationToken);

    if (interaction is null ||
        !interaction.university_id.HasValue)
    {
        return false;
    }

    var hasAccess =
        await _accessService
            .HasAccessToUniversityAsync(
                interaction.university_id.Value,
                cancellationToken);

    if (!hasAccess)
    {
        return false;
    }

    if (!interaction.current_status_id.HasValue)
    {
        throw new InvalidOperationException(
            "У interaction отсутствует текущий статус.");
    }

    var databaseUserId =
        await _accessService
            .GetCurrentDatabaseUserIdAsync(cancellationToken);

    // Комментарий без смены статуса: from = to = текущий статус.
    // dto.StatusId на запись не влияет — привязка видна по текущему этапу.
    var history =
        new interaction_status_history
        {
            interaction_id =
                interaction.interactions_id,

            from_status_id =
                interaction.current_status_id,

            to_status_id =
                interaction.current_status_id,

            changed_by =
                databaseUserId,

            comment =
                dto.Comment.Trim(),

            changed_at =
                DateTime.UtcNow
        };

    _context.interaction_status_histories.Add(history);

    await _context.SaveChangesAsync(cancellationToken);

    await _auditService.WriteAsync(
        "COMMENT",
        "interaction",
        interaction.interactions_id,
        null,
        new { Comment = dto.Comment.Trim() },
        cancellationToken);

    return true;
}
}