using System.Text.Json;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Responsible;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class ResponsiblesService : IResponsiblesService
{
    private readonly CrmDbContext _db;

    public ResponsiblesService(CrmDbContext db)
    {
        _db = db;
    }

    // ---------------------------------------------------------------
    // GET managers
    // ---------------------------------------------------------------
    public async Task<List<ResponsibleManagerDto>> GetManagersAsync(
        CancellationToken ct = default)
    {
        // Пользователи, которые уже фигурируют как ответственные:
        // закреплены за вузами (university_managers) и/или назначены
        // менеджером взаимодействия (interactions.manager_id).
        // Так назначения, сделанные со страницы «Вузы»
        // (PUT /Interactions/{id}), здесь не теряются.
        // ВАЖНО: user_id и manager_id в заскафолденных сущностях — int?,
        // поэтому сначала отсекаем NULL, потом берём .Value —
        // иначе Union не скомпилируется (IQueryable<int?> vs IQueryable<int>).
        var responsibleUserIds = await _db.university_managers
            .Where(um => um.user_id != null)
            .Select(um => um.user_id!.Value)
            .Union(_db.interactions
                .Where(i => i.manager_id != null)
                .Select(i => i.manager_id!.Value))
            .Distinct()
            .ToListAsync(ct);

        if (responsibleUserIds.Count == 0)
        {
            return new List<ResponsibleManagerDto>();
        }

        var users = await _db.users
            .Where(u => responsibleUserIds.Contains(u.users_id))
            .OrderBy(u => u.last_name)
            .ThenBy(u => u.first_name)
            .ToListAsync(ct);

        var assignments = await _db.university_managers
            .Where(um => um.user_id != null && responsibleUserIds.Contains(um.user_id!.Value))
            .Join(_db.universities,
                um => um.university_id,
                uni => uni.universities_id,
                (um, uni) => new
                {
                    UserId = um.user_id!.Value,
                    UniversityId = uni.universities_id,
                    UniversityName = uni.name,
                })
            .ToListAsync(ct);

        return users.Select(u =>
        {
            var unis = assignments
                .Where(a => a.UserId == u.users_id)
                .OrderBy(a => a.UniversityName)
                .ToList();

            return new ResponsibleManagerDto
            {
                Id = u.users_id,
                FullName = BuildFullName(u),
                Email = u.email,
                // user.is_active — bool? (так заскафолдило из БД)
                IsActive = u.is_active ?? false,
                UniversitiesCount = unis.Count,
                Universities = unis.Select(a => new ResponsibleUniversityDto
                {
                    Id = a.UniversityId,
                    Name = a.UniversityName,
                }).ToList(),
            };
        }).ToList();
    }

    // ---------------------------------------------------------------
    // PUT managers/{userId}/universities
    // ---------------------------------------------------------------
    public async Task<ResponsibleManagerDto> UpdateManagerUniversitiesAsync(
        int userId,
        UpdateManagerUniversitiesDto dto,
        int? actingUserId,
        CancellationToken ct = default)
    {
        var user = await _db.users.FirstOrDefaultAsync(u => u.users_id == userId, ct)
            ?? throw new KeyNotFoundException($"Пользователь с id={userId} не найден.");

        var requestedIds = (dto.UniversityIds ?? new List<int>())
            .Distinct()
            .ToList();

        if (requestedIds.Count > 0)
        {
            var existingIds = await _db.universities
                .Where(x => requestedIds.Contains(x.universities_id))
                .Select(x => x.universities_id)
                .ToListAsync(ct);

            var missing = requestedIds.Except(existingIds).ToList();
            if (missing.Count > 0)
            {
                throw new InvalidDataException(
                    $"Вузы не найдены: {string.Join(", ", missing)}.");
            }
        }

        await using var tx = await _db.Database
            .BeginTransactionAsync(ct);

        var oldAssignments = await _db.university_managers
            .Where(um => um.user_id == userId)
            .Select(um => new { um.university_id })
            .ToListAsync(ct);

        _db.university_managers.RemoveRange(
            _db.university_managers.Where(um => um.user_id == userId));

        foreach (var universityId in requestedIds)
        {
            _db.university_managers.Add(new university_manager
            {
                user_id = userId,
                university_id = universityId,
                assigned_at = DateTime.UtcNow,
                is_primary = false,
            });
        }

        // ВАЖНО: в заскафолденной сущности audit_log поля
        // old_data/new_data — string (jsonb), НЕ JsonDocument:
        // сериализуем напрямую, без JsonDocument.Parse.
        _db.audit_logs.Add(new audit_log
        {
            user_id = actingUserId,
            action = "manager_universities_reassigned",
            entity_type = "user",
            entity_id = userId,
            old_data = JsonSerializer.Serialize(
                oldAssignments.Select(a => a.university_id).OrderBy(x => x)),
            new_data = JsonSerializer.Serialize(
                requestedIds.OrderBy(x => x)),
            created_at = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return await BuildManagerDtoAsync(userId, ct);
    }

    private async Task<ResponsibleManagerDto> BuildManagerDtoAsync(
        int userId, CancellationToken ct)
    {
        var user = await _db.users.FirstAsync(u => u.users_id == userId, ct);

        var unis = await _db.university_managers
            .Where(um => um.user_id == userId)
            .Join(_db.universities,
                um => um.university_id,
                x => x.universities_id,
                (um, x) => new ResponsibleUniversityDto { Id = x.universities_id, Name = x.name })
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return new ResponsibleManagerDto
        {
            Id = user.users_id,
            FullName = BuildFullName(user),
            Email = user.email,
            // user.is_active — bool?
            IsActive = user.is_active ?? false,
            UniversitiesCount = unis.Count,
            Universities = unis,
        };
    }

    // ---------------------------------------------------------------
    // GET contacts
    // ---------------------------------------------------------------
    public async Task<List<UniversityContactDto>> GetContactsAsync(
        CancellationToken ct = default)
    {
        // Все контакты (активные и неактивные) — колонка «Статус»
        // в таблице должна быть содержательной.
        // Типы заскафолденной сущности university_contact:
        //   university_contacts_id — int (PK, НЕ nullable),
        //   university_id — int? (FK, nullable),
        //   is_active — bool? (nullable) — маппим с coalesce.
        return await _db.university_contacts
            .GroupJoin(_db.universities,
                c => c.university_id,
                u => u.universities_id,
                (c, us) => new { Contact = c, Universities = us })
            .SelectMany(x => x.Universities.DefaultIfEmpty(),
                (x, u) => new UniversityContactDto
                {
                    Id = x.Contact.university_contacts_id,
                    UniversityId = x.Contact.university_id ?? 0,
                    UniversityName = u != null ? (u.short_name ?? u.name) : null,
                    FullName = x.Contact.full_name,
                    Position = x.Contact.position,
                    Email = x.Contact.email,
                    Phone = x.Contact.phone,
                    IsActive = x.Contact.is_active ?? false,
                    Comment = x.Contact.comment,
                })
            .OrderBy(x => x.FullName)
            .ToListAsync(ct);
    }

    // ---------------------------------------------------------------
    // POST contacts
    // ---------------------------------------------------------------
    public async Task<UniversityContactDto> CreateContactAsync(
        SaveUniversityContactDto dto,
        int? actingUserId,
        CancellationToken ct = default)
    {
        ValidateContact(dto);

        var universityExists = await _db.universities
            .AnyAsync(u => u.universities_id == dto.UniversityId, ct);
        if (!universityExists)
        {
            throw new InvalidDataException("Выбранный вуз не найден.");
        }

        var contact = new university_contact
        {
            university_id = dto.UniversityId,
            full_name = dto.FullName.Trim(),
            position = string.IsNullOrWhiteSpace(dto.Position) ? null : dto.Position.Trim(),
            email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
            is_active = dto.IsActive,
            comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
        };

        _db.university_contacts.Add(contact);

        _db.audit_logs.Add(new audit_log
        {
            user_id = actingUserId,
            action = "university_contact_created",
            entity_type = "university_contact",
            entity_id = 0, // id ещё нет; данные фиксируем в new_data
            new_data = JsonSerializer.Serialize(new
            {
                contact.university_id,
                contact.full_name,
                contact.position,
                contact.email,
                contact.phone,
                contact.is_active,
            }),
            created_at = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(ct);

        return await BuildContactDtoAsync(contact, ct);
    }

    // ---------------------------------------------------------------
    // PUT contacts/{id}
    // ---------------------------------------------------------------
    public async Task<UniversityContactDto> UpdateContactAsync(
        int id,
        SaveUniversityContactDto dto,
        int? actingUserId,
        CancellationToken ct = default)
    {
        var contact = await _db.university_contacts
            .FirstOrDefaultAsync(c => c.university_contacts_id == id, ct)
            ?? throw new KeyNotFoundException($"Представитель с id={id} не найден.");

        ValidateContact(dto);

        var universityExists = await _db.universities
            .AnyAsync(u => u.universities_id == dto.UniversityId, ct);
        if (!universityExists)
        {
            throw new InvalidDataException("Выбранный вуз не найден.");
        }

        var oldSnapshot = new
        {
            contact.university_id,
            contact.full_name,
            contact.position,
            contact.email,
            contact.phone,
            contact.is_active,
            contact.comment,
        };

        contact.university_id = dto.UniversityId;
        contact.full_name = dto.FullName.Trim();
        contact.position = string.IsNullOrWhiteSpace(dto.Position) ? null : dto.Position.Trim();
        contact.email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        contact.phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
        contact.is_active = dto.IsActive;
        contact.comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();

        _db.audit_logs.Add(new audit_log
        {
            user_id = actingUserId,
            action = "university_contact_updated",
            entity_type = "university_contact",
            // university_contacts_id — int, coalesce не нужен
            entity_id = contact.university_contacts_id,
            old_data = JsonSerializer.Serialize(oldSnapshot),
            new_data = JsonSerializer.Serialize(new
            {
                contact.university_id,
                contact.full_name,
                contact.position,
                contact.email,
                contact.phone,
                contact.is_active,
                contact.comment,
            }),
            created_at = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(ct);

        return await BuildContactDtoAsync(contact, ct);
    }

    // ---------------------------------------------------------------
    // DELETE contacts/{id} — мягкое удаление (is_active = false)
    // ---------------------------------------------------------------
    public async Task DeleteContactAsync(
        int id, int? actingUserId, CancellationToken ct = default)
    {
        var contact = await _db.university_contacts
            .FirstOrDefaultAsync(c => c.university_contacts_id == id, ct)
            ?? throw new KeyNotFoundException($"Представитель с id={id} не найден.");

        // is_active — bool?: проверяем через != true, чтобы не писать
        // !contact.is_active (оператор ! над bool? не компилируется).
        if (contact.is_active != true)
        {
            return; // идемпотентно: повторный вызов — тихий успех
        }

        contact.is_active = false;

        _db.audit_logs.Add(new audit_log
        {
            user_id = actingUserId,
            action = "university_contact_deactivated",
            entity_type = "university_contact",
            // university_contacts_id — int, coalesce не нужен
            entity_id = contact.university_contacts_id,
            old_data = JsonSerializer.Serialize(new { is_active = true }),
            new_data = JsonSerializer.Serialize(new { is_active = false }),
            created_at = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(ct);
    }

    // ---------------------------------------------------------------
    // Резолв текущего пользователя для аудита
    // ---------------------------------------------------------------
    public async Task<int?> ResolveUserIdByKeycloakSubAsync(
        string keycloakSub, CancellationToken ct = default)
    {
        return await _db.users
            .Where(u => u.keycloak_user_id == keycloakSub)
            .Select(u => (int?)u.users_id)
            .FirstOrDefaultAsync(ct);
    }

    // ---------------------------------------------------------------
    // Хелперы
    // ---------------------------------------------------------------
    private async Task<UniversityContactDto> BuildContactDtoAsync(
        university_contact contact, CancellationToken ct)
    {
        var universityName = await _db.universities
            .Where(u => u.universities_id == contact.university_id)
            .Select(u => (string?)(u.short_name ?? u.name))
            .FirstOrDefaultAsync(ct);

        return new UniversityContactDto
        {
            // university_contacts_id — int (PK), university_id — int?
            Id = contact.university_contacts_id,
            UniversityId = contact.university_id ?? 0,
            UniversityName = universityName,
            FullName = contact.full_name,
            Position = contact.position,
            Email = contact.email,
            Phone = contact.phone,
            IsActive = contact.is_active ?? false,
            Comment = contact.comment,
        };
    }

    private static string BuildFullName(user u)
    {
        return string.Join(" ",
            new[] { u.last_name, u.first_name, u.middle_name }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
    }

    private static void ValidateContact(SaveUniversityContactDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName) || dto.FullName.Trim().Length < 2)
        {
            throw new InvalidDataException("ФИО обязательно (не короче 2 символов).");
        }

        if (dto.FullName.Length > 200)
        {
            throw new InvalidDataException("ФИО не должно превышать 200 символов.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) && !dto.Email.Contains('@'))
        {
            throw new InvalidDataException("Некорректный email.");
        }

        if (dto.Phone != null && dto.Phone.Length > 50)
        {
            throw new InvalidDataException("Телефон не должен превышать 50 символов.");
        }
    }
}