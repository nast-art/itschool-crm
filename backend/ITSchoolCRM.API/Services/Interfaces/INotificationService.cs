using ITSchoolCRM.API.DTOs.Notifications;  
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
        Task<IReadOnlyList<NotificationDto>> GetForUserAsync(int userId, IReadOnlyCollection<string> roles, CancellationToken ct);

        Task<int?> ResolveUserIdAsync(string keycloakUserId, CancellationToken ct);

        Task MarkReadAsync(int userId, string notificationId, CancellationToken ct);

        Task MarkAllReadAsync(int userId, IReadOnlyCollection<string> roles, CancellationToken ct);
    }
}