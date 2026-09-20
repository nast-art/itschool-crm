using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Users;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

public class UserService : IUserService
{
    private readonly CrmDbContext _context;

    public UserService(
        CrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserDto>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _context.users
            .AsNoTracking()
            .Where(x => x.is_active == true)
            .OrderBy(x => x.last_name)
            .Select(x => new UserDto
            {
                Id = x.users_id,
                KeycloakUserId = x.keycloak_user_id,
                LastName = x.last_name,
                FirstName = x.first_name,
                MiddleName = x.middle_name,
                Email = x.email,
                IsActive = x.is_active
            })
            .ToListAsync(cancellationToken);
    }
}