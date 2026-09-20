using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Licenses;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LicensesController : ControllerBase
{
    private readonly ILicenseService _service;

    public LicensesController(
        ILicenseService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<LicenseDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var licenses =
            await _service.GetAllAsync(
                cancellationToken);

        return Ok(licenses);
    }
}