using ITSchoolCRM.API.DTOs.Contracts;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IContractService
{
    Task<List<ContractDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<List<ContractInteractionOptionDto>> GetInteractionOptionsAsync(CancellationToken cancellationToken);

    Task<ContractDto> CreateAsync(ContractCreateDto dto, CancellationToken cancellationToken);

    Task<ContractDto?> UpdateAsync(int id, ContractUpdateDto dto, CancellationToken cancellationToken);
}