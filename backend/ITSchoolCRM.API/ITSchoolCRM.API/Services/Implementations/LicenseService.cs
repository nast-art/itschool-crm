using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Licenses;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class LicenseService : ILicenseService
{
    private readonly CrmDbContext _context;

    public LicenseService(
        CrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<LicenseDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.licenses
            .AsNoTracking()
            .OrderByDescending(x => x.signed_at)
            .Select(x => new LicenseDto
            {
                Id = x.licenses_id,
                SignedAt = x.signed_at,
                ValidUntil = x.valid_until,
                TransferStatus = x.transfer_status,
                Comment = x.comment
            })
            .ToListAsync(cancellationToken);
    }
}