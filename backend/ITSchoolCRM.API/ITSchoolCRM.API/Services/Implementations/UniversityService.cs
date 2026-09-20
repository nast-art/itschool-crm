using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Universities;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class UniversityService : IUniversityService
{
    private readonly CrmDbContext _context;
    private readonly IUserAccessService _accessService;

    public UniversityService(
        CrmDbContext context,
        IUserAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
    }

    public async Task<List<UniversityDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var accessibleUniversityIds =
            _accessService.GetAccessibleUniversityIds();

        return await _context.universities
            .AsNoTracking()
            .Where(x =>
                accessibleUniversityIds.Contains(
                    x.universities_id))
            .Select(x => new UniversityDto
            {
                Id = x.universities_id,

                Name = x.name,

                ShortName = x.short_name,

                IsActive = x.is_active
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<UniversityDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var accessible =
            await _accessService
                .HasAccessToUniversityAsync(
                    id,
                    cancellationToken);

        if (!accessible)
        {
            return null;
        }

        return await _context.universities
            .AsNoTracking()
            .Where(x =>
                x.universities_id == id)
            .Select(x => new UniversityDto
            {
                Id = x.universities_id,

                Name = x.name,

                ShortName = x.short_name,

                IsActive = x.is_active
            })
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    public async Task<List<UniversityDto>> GetActiveAsync(
        CancellationToken cancellationToken)
    {
        var accessibleUniversityIds =
            _accessService.GetAccessibleUniversityIds();

        return await _context.universities
            .AsNoTracking()
            .Where(x =>
                x.is_active == true
                &&
                accessibleUniversityIds.Contains(
                    x.universities_id))
            .Select(x => new UniversityDto
            {
                Id = x.universities_id,

                Name = x.name,

                ShortName = x.short_name,

                IsActive = x.is_active
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UniversityDto>> SearchAsync(
        string? search,
        CancellationToken cancellationToken)
    {
        var accessibleUniversityIds =
            _accessService.GetAccessibleUniversityIds();

        var query = _context.universities
            .AsNoTracking()
            .Where(x =>
                accessibleUniversityIds.Contains(
                    x.universities_id))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            var lowerSearch =
                search.ToLower();

            query = query.Where(x =>
                (x.name != null &&
                 x.name.ToLower()
                    .Contains(lowerSearch))

                ||

                (x.short_name != null &&
                 x.short_name.ToLower()
                    .Contains(lowerSearch)));
        }

        return await query
            .Select(x => new UniversityDto
            {
                Id = x.universities_id,

                Name = x.name,

                ShortName = x.short_name,

                IsActive = x.is_active
            })
            .ToListAsync(
                cancellationToken);
    }

    public async Task<UniversityDto> CreateAsync(
        CreateUniversityDto dto,
        CancellationToken cancellationToken)
    {
        var university = new university
        {
            name = dto.Name,

            short_name = dto.ShortName,

            is_active = true,

            created_at = DateTime.UtcNow
        };

        _context.universities.Add(
            university);

        await _context.SaveChangesAsync(
            cancellationToken);

        return new UniversityDto
        {
            Id = university.universities_id,

            Name = university.name,

            ShortName = university.short_name,

            IsActive = university.is_active
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateUniversityDto dto,
        CancellationToken cancellationToken)
    {
        var university =
            await _context.universities
                .FirstOrDefaultAsync(
                    x =>
                        x.universities_id == id,
                    cancellationToken);

        if (university is null)
        {
            return false;
        }

        university.name =
            dto.Name;

        university.short_name =
            dto.ShortName;

        university.is_active =
            dto.IsActive;

        university.updated_at =
            DateTime.UtcNow;

        await _context.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var university =
            await _context.universities
                .FirstOrDefaultAsync(
                    x =>
                        x.universities_id == id,
                    cancellationToken);

        if (university is null)
        {
            return false;
        }

        university.is_active = false;

        university.updated_at =
            DateTime.UtcNow;

        await _context.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}