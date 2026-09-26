using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Разграничение доступа к данным по вузам (152-ФЗ).
/// manager/admin видят всё; менеджер по вузам — только закреплённые
/// за ним через university_managers (связь с users — по keycloak_user_id
/// из JWT, без предварительного резолва users_id: минус один запрос).
/// </summary>
public class UserAccessService : IUserAccessService
{
    private readonly CrmDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UserAccessService(
        CrmDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public bool HasFullAccess()
    {
        return _currentUser.IsManager ||
               _currentUser.IsAdmin;
    }

    public async Task<int?> GetCurrentDatabaseUserIdAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUser.KeycloakUserId))
        {
            return null;
        }

        return await _context.users
            .AsNoTracking()
            .Where(x => x.keycloak_user_id == _currentUser.KeycloakUserId)
            .Select(x => (int?)x.users_id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public IQueryable<int> GetAccessibleUniversityIds()
    {
        // Полный доступ — все вузы без фильтра
        if (HasFullAccess())
        {
            return _context.universities
                .AsNoTracking()
                .Select(x => x.universities_id);
        }

        // Не аутентифицирован — пустой набор (контроллеры под [Authorize],
        // но защищаемся): без этой ветки запрос упал бы на null-сравнении
        if (string.IsNullOrWhiteSpace(_currentUser.KeycloakUserId))
        {
            return Enumerable.Empty<int>().AsQueryable();
        }

        return
            from universityManager in _context.university_managers
            join user in _context.users
                on universityManager.user_id equals user.users_id
            where universityManager.university_id.HasValue
                  && user.keycloak_user_id == _currentUser.KeycloakUserId
            select universityManager.university_id.Value;
    }

    public async Task<bool> HasAccessToUniversityAsync(
        int universityId,
        CancellationToken cancellationToken)
    {
        if (HasFullAccess())
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(_currentUser.KeycloakUserId))
        {
            return false;
        }

        return await (
            from universityManager in _context.university_managers
            join user in _context.users
                on universityManager.user_id equals user.users_id
            where universityManager.university_id == universityId
                  && user.keycloak_user_id == _currentUser.KeycloakUserId
            select universityManager.university_managers_id)
        .AnyAsync(cancellationToken);
    }
}