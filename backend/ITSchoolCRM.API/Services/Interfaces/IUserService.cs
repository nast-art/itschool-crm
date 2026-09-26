using ITSchoolCRM.API.DTOs.Users;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken);
}