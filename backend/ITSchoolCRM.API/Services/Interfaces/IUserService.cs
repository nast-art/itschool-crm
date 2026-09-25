using ITSchoolCRM.API.DTOs.Users;

namespace ITSchoolCRM.API.Services.Interfaces;

public interface IUserService
{
    // Активные пользователи CRM — для фильтра «Ответственный»
    // и назначения менеджеров (п. 11 ТЗ)
    Task<List<UserDto>> GetAllAsync(
        CancellationToken cancellationToken);
}