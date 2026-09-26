using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Audit;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditService _service;

    public AuditController(IAuditService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<ActionResult<List<AuditLogDto>>> GetAll(CancellationToken cancellationToken)
    {
        var logs = await _service.GetAllAsync(cancellationToken);

        return Ok(logs);
    }

    [HttpGet("{entityType}/{entityId:int}")]
    [Authorize(Policy = Policies.AdminAccess)]
    public async Task<ActionResult<List<AuditLogDto>>> GetByEntity(string entityType, int entityId, CancellationToken cancellationToken)
    {
        var logs = await _service.GetByEntityAsync( entityType, entityId, cancellationToken);

        return Ok(logs);
    }
}