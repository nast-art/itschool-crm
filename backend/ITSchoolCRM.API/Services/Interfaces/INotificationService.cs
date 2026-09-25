using ITSchoolCRM.API.DTOs.Notifications;   // было: using ITSchoolCRM.API.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.Services.Interfaces
{
    /// <summary>
    /// Сервис уведомлений. Уведомления ВЫЧИСЛЯЮТСЯ из данных CRM
    /// (отдельной таблицы нет): зависшие взаимодействия и истекающие
    /// лицензии. Факт прочтения хранится в IMemoryCache по паре
    /// (userId, notificationId) — допустимо для демо-контура.
    ///
    /// Видимость (152-ФЗ): admin видит всё, остальные — только свои
    /// взаимодействия (manager_id = user) и вузы через university_managers.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>Уведомления для пользователя, от новых к старым.</summary>
        Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
            int userId, IReadOnlyCollection<string> roles, CancellationToken ct);

        /// <summary>users.users_id по Keycloak sub (связь через keycloak_user_id).</summary>
        Task<int?> ResolveUserIdAsync(string keycloakUserId, CancellationToken ct);

        /// <summary>Отметить одно уведомление прочитанным.</summary>
        Task MarkReadAsync(int userId, string notificationId, CancellationToken ct);

        /// <summary>Отметить все ТЕКУЩИЕ уведомления прочитанными.
        /// Роли нужны, чтобы не-админ не «потерял» часть списка.</summary>
        Task MarkAllReadAsync(int userId, IReadOnlyCollection<string> roles, CancellationToken ct);
    }
}