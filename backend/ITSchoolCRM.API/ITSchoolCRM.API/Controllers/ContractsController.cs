using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Contracts;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _service;

    public ContractsController(
        IContractService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<ContractDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var contracts =
            await _service.GetAllAsync(
                cancellationToken);

        return Ok(contracts);
    }
}