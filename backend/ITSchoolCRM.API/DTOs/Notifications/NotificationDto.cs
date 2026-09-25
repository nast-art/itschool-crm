using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITSchoolCRM.API.DTOs.Notifications
{
    /// <summary>
    /// Уведомление пользователя (ответ GET /Notifications).
    /// Имена полей — camelCase (JsonNamingPolicy настроена в Program.cs),
    /// фронт читает именно их; snake_case тоже переживём не будем —
    /// сериализуем строго в camelCase.
    /// </summary>
    public class NotificationDto
    {
        /// <summary>Стабильный строковый id: "stale-{interactionId}", "license-{interactionId}".
        /// Строка, а не число: уведомления вычисляемые, а не строки таблицы.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Тип: stale_status | license_expiring (метки на фронте сопоставлены).</summary>
        public string Type { get; set; } = "system";

        public string Title { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        /// <summary>Время события (последняя смена статуса / дата истечения лицензии), UTC.</summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>Прочитано ли текущим пользователем (состояние из IMemoryCache).</summary>
        public bool IsRead { get; set; }

        /// <summary>Маршрут фронта для перехода к сути уведомления.</summary>
        public string? Link { get; set; }
    }
}