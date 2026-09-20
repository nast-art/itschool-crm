using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.Services.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITSchoolCRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly ICurrentUserService _currentUser;

    public AccountController(
        ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            keycloakUserId =
                _currentUser.KeycloakUserId,

            userName =
                _currentUser.UserName,

            email =
                _currentUser.Email,

            roles =
                _currentUser.Roles
        });
    }

    [HttpGet("user")]
    [Authorize(Policy = Policies.UserAccess)]
    public IActionResult UserAccess()
    {
        return Ok(new
        {
            message =
                "У пользователя есть доступ к пользовательской части API."
        });
    }

    [HttpGet("manager")]
    [Authorize(Policy = Policies.ManagerAccess)]
    public IActionResult ManagerAccess()
    {
        return Ok(new
        {
            message =
                "У пользователя есть права руководителя."
        });
    }

    [HttpGet("admin")]
    [Authorize(Policy = Policies.AdminAccess)]
    public IActionResult AdminAccess()
    {
        return Ok(new
        {
            message =
                "У пользователя есть права администратора."
        });
    }
}