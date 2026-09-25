using System.Security.Claims;
using ITSchoolCRM.API.Auth;
using ITSchoolCRM.API.Services.Interfaces;

namespace ITSchoolCRM.API.Services.Implementations;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public string? KeycloakUserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue("sub");

    public string? UserName =>
        User?.FindFirstValue(
            ClaimTypes.Name)
        ?? User?.FindFirstValue("preferred_username");

    public string? Email =>
        User?.FindFirstValue(
            ClaimTypes.Email)
        ?? User?.FindFirstValue("email");

    public string? FullName =>
        User?.FindFirstValue("name");

    public IReadOnlyList<string> Roles =>
        User?
            .FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .Distinct()
            .ToList()
        ?? new List<string>();

    public bool IsUser =>
        User?.IsInRole(ITSchoolCRM.API.Auth.Roles.User) == true;

    public bool IsManager =>
        User?.IsInRole(ITSchoolCRM.API.Auth.Roles.Manager) == true;

    public bool IsAdmin =>
        User?.IsInRole(ITSchoolCRM.API.Auth.Roles.Admin) == true;
}