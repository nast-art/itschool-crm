using ITSchoolCRM.API.DTOs.Contracts;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IContractService
{
    // Справочник договоров — колонка «№ Договора» на странице «Вузы»
    Task<List<ContractDto>> GetAllAsync(
        CancellationToken cancellationToken);
}