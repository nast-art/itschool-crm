using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.DTOs.Users;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(
        IUserService service)
    {
        _service = service;
    }

    // Пользователи CRM: фильтр «Ответственный», назначение менеджеров,
    // модалка «Добавить вуз». ФИО фронт собирает из частей.
    [HttpGet]
    [Authorize(Policy = Policies.UserAccess)]
    public async Task<ActionResult<List<UserDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var users =
            await _service.GetAllAsync(
                cancellationToken);

        return Ok(users);
    }
}