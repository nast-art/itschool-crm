using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITSchoolCRM.API.Services.Implementations;

namespace ITSchoolCRM.API.Services.Interfaces
{
    /// <summary>
    /// Административный доступ к Keycloak: пользователи с ролями,
    /// назначение роли, блокировка/разблокировка учётной записи.
    /// Единственная реализация — <see cref="KeycloakAdminClient"/>.
    /// </summary>
    public interface IKeycloakAdminClient
    {
        /// <summary>
        /// Все пользователи realm с их ролью CRM (user/manager/admin).
        /// Если пользователь не состоит ни в одной роли — role = "user".
        /// </summary>
        Task<IReadOnlyList<KeycloakUserRecord>> ListUsersWithRolesAsync(
            CancellationToken ct);

        /// <summary>
        /// Заменяет realm-роль CRM пользователя: снимает две остальные
        /// роли и назначает новую. Идемпотентно.
        /// </summary>
        Task UpdateUserRoleAsync(
            string keycloakUserId, string newRole, CancellationToken ct);

        /// <summary>
        /// Включает/выключает учётную запись в Keycloak (флаг enabled).
        /// </summary>
        Task SetUserEnabledAsync(
            string keycloakUserId, bool enabled, CancellationToken ct);
    }
}