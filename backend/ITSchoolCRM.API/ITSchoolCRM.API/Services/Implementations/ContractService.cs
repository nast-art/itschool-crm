using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Contracts;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class ContractService : IContractService
{
    private readonly CrmDbContext _context;

    public ContractService(
        CrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContractDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.contracts
            .AsNoTracking()
            .OrderByDescending(x => x.signed_at)
            .Select(x => new ContractDto
            {
                Id = x.contracts_id,
                ContractNumber = x.contract_number,
                SignedAt = x.signed_at,
                Status = x.status,
                Comment = x.comment
            })
            .ToListAsync(cancellationToken);
    }
}