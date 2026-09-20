using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Programs;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class ProgramService : IProgramService
{
    private readonly CrmDbContext _context;

    public ProgramService(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProgramDto>> GetAllAsync(
        int? directionId,
        CancellationToken cancellationToken)
    {
        var query = _context.it_programs
            .AsNoTracking()
            .AsQueryable();

        if (directionId.HasValue)
        {
            query = query.Where(
                x => x.direction_id == directionId.Value);
        }

        return await query
            .Select(x => new ProgramDto
            {
                Id = x.it_programs_id,
                DirectionId = x.direction_id,
                Name = x.name,
                Description = x.description,
                IsActive = x.is_active
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ProgramDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _context.it_programs
            .AsNoTracking()
            .Where(x => x.it_programs_id == id)
            .Select(x => new ProgramDto
            {
                Id = x.it_programs_id,
                DirectionId = x.direction_id,
                Name = x.name,
                Description = x.description,
                IsActive = x.is_active
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ProgramDto> CreateAsync(
        CreateProgramDto dto,
        CancellationToken cancellationToken)
    {
        var program = new it_program
        {
            direction_id = dto.DirectionId,
            name = dto.Name,
            description = dto.Description,
            is_active = true,
            created_at = DateTime.UtcNow
        };

        _context.it_programs.Add(program);

        await _context.SaveChangesAsync(cancellationToken);

        return new ProgramDto
        {
            Id = program.it_programs_id,
            DirectionId = program.direction_id,
            Name = program.name,
            Description = program.description,
            IsActive = program.is_active
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateProgramDto dto,
        CancellationToken cancellationToken)
    {
        var program = await _context.it_programs
            .FirstOrDefaultAsync(
                x => x.it_programs_id == id,
                cancellationToken);

        if (program is null)
        {
            return false;
        }

        program.direction_id = dto.DirectionId;
        program.name = dto.Name;
        program.description = dto.Description;
        program.is_active = dto.IsActive;
        program.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var program = await _context.it_programs
            .FirstOrDefaultAsync(
                x => x.it_programs_id == id,
                cancellationToken);

        if (program is null)
        {
            return false;
        }

        program.is_active = false;
        program.updated_at = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<List<ProgramDto>> GetActiveAsync(
    int? directionId,
    CancellationToken cancellationToken)
{
    var query = _context.it_programs
        .AsNoTracking()
        .Where(x => x.is_active == true)
        .AsQueryable();

    if (directionId.HasValue)
    {
        query = query.Where(
            x => x.direction_id == directionId.Value);
    }

    return await query
        .Select(x => new ProgramDto
        {
            Id = x.it_programs_id,
            DirectionId = x.direction_id,
            Name = x.name,
            Description = x.description,
            IsActive = x.is_active
        })
        .ToListAsync(cancellationToken);
}

public async Task<List<ProgramDto>> SearchAsync(
    string? search,
    int? directionId,
    CancellationToken cancellationToken)
{
    var query = _context.it_programs
        .AsNoTracking()
        .AsQueryable();

    if (directionId.HasValue)
    {
        query = query.Where(
            x => x.direction_id == directionId.Value);
    }

    if (!string.IsNullOrWhiteSpace(search))
    {
        search = search.Trim();

        query = query.Where(x =>
            x.name != null &&
            x.name.ToLower().Contains(search.ToLower()));
    }

    return await query
        .Select(x => new ProgramDto
        {
            Id = x.it_programs_id,
            DirectionId = x.direction_id,
            Name = x.name,
            Description = x.description,
            IsActive = x.is_active
        })
        .ToListAsync(cancellationToken);
}
}