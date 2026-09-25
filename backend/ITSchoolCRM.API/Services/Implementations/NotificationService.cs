using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.DTOs.Notifications;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations
{
    /// <summary>
    /// Уведомления: зависшие заявки (>14 дней без смены статуса) и
    /// лицензии (истекшие + ≤30 дней до истечения).
    ///
    /// Уведомления НЕ хранятся в БД — каждый раз вычисляются из данных CRM
    /// со стабильными id ("stale-{interactions_id}", "license-{licenses_id}"),
    /// поэтому переживают перезапуск и релогин. Пометки «прочитано» бэкенд
    /// не хранит: это персональный UI-кэш на фронте (localStorage, ключ
    /// с субъектом JWT) — нефункциональное требование 13 ТЗ.
    ///
    /// Ссылки — глубокие: зависшая заявка → /interactions?focus={id};
    /// лицензия → /contracts?focus={contractId}. Договор разрешается
    /// через взаимодействие лицензии (interactions.license_id →
    /// interactions.contract_id); если оно не привязано к договору —
    /// ищется ЛЮБОЕ взаимодействие этого же вуза с договором. Если и
    /// это не найдено — общая ссылка /contracts (страница просто
    /// откроется без подсветки).
    ///
    /// Разграничение по данным (п. 11 ТЗ): manager/admin видят всё,
    /// user — только свои вузы (interactions.manager_id).
    /// </summary>
    public class NotificationService : INotificationService
    {
        // Порог «зависания» заявки (уточнение заказчика 16.09.2026 12:41)
        private static readonly TimeSpan StaleThreshold = TimeSpan.FromDays(14);

        // Горизонт предупреждения по лицензиям
        private static readonly TimeSpan LicenseWarningWindow = TimeSpan.FromDays(30);

        // Верхний предел выдачи — чтобы панель не росла бесконечно
        private const int MaxNotifications = 50;

        private readonly CrmDbContext _db;

        public NotificationService(CrmDbContext db)
        {
            _db = db;
        }

        // ---------- INotificationService ----------

        /// <inheritdoc />
        public async Task<int?> ResolveUserIdAsync(
            string keycloakUserId, CancellationToken ct)
        {
            var user = await _db.users
                .FirstOrDefaultAsync(
                    u => u.keycloak_user_id == keycloakUserId && u.is_active == true,
                    ct);
            return user?.users_id;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
            int userId, IReadOnlyCollection<string> roles, CancellationToken ct)
        {
            var notifications = await BuildNotificationsAsync(userId, roles, ct);

            return notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(MaxNotifications)
                .ToList();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Персистентной пометки «прочитано» на бэкенде нет (кэш на фронте),
        /// поэтому метод — намеренный no-op: контракт с фронтом сохранён,
        /// лишней работы и состояния — нет.
        /// </remarks>
        public Task MarkReadAsync(
            int userId, string id, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        /// <remarks>См. <see cref="MarkReadAsync"/> — намеренный no-op.</remarks>
        public Task MarkAllReadAsync(
            int userId, IReadOnlyCollection<string> roles, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        // ---------- Построение списка ----------

        private async Task<List<NotificationDto>> BuildNotificationsAsync(
            int userId, IReadOnlyCollection<string> roles, CancellationToken ct)
        {
            // Разграничение по данным (п. 11 ТЗ)
            var isPrivileged =
                roles.Contains("admin") || roles.Contains("manager");

            var now = DateTime.UtcNow;
            var list = new List<NotificationDto>();

            // 1) Зависшие заявки: не финальный статус, updated_at старше 14 дней
            var staleBefore = now - StaleThreshold;
            var stale = await _db.interactions
                .Include(i => i.university)
                .Include(i => i.program)
                .Include(i => i.current_status)
                .Where(i =>
                    i.current_status != null &&
                    i.current_status.is_final != true &&
                    i.updated_at != null &&
                    i.updated_at < staleBefore)
                .ToListAsync(ct);

            if (!isPrivileged)
            {
                stale = stale.Where(i => i.manager_id == userId).ToList();
            }

            foreach (var i in stale)
            {
                list.Add(new NotificationDto
                {
                    Id = $"stale-{i.interactions_id}",
                    Type = "stale_status",
                    Title = "Заявка зависла более чем на 14 дней",
                    Text = $"{i.university?.name ?? "Вуз"} · " +
                           $"{i.program?.name ?? "Программа"} — статус " +
                           $"«{i.current_status?.name ?? "—"}» без изменений с " +
                           $"{i.updated_at:dd.MM.yyyy}. Уточните причину задержки.",
                    CreatedAt = i.updated_at ?? i.created_at,
                    IsRead = false,
                    // Глубокая ссылка: страница «Взаимодействия» выделит
                    // именно эту карточку (поддержка ?focus= в фронте)
                    Link = $"/interactions?focus={i.interactions_id}",
                });
            }

            // 2) Лицензии: просроченные + с истечением ≤30 дней.
            // Вуз определяем через связанные взаимодействия
            // (interactions.license_id) с учётом видимости пользователя.
            var expireBefore = now + LicenseWarningWindow;
            var licenses = await _db.licenses
                .Include(l => l.interactions)
                    .ThenInclude(i => i.university)
                .Where(l => l.valid_until != null && l.valid_until <= expireBefore)
                .ToListAsync(ct);

            foreach (var l in licenses)
            {
                var visible = l.interactions
                    .Where(i => isPrivileged || i.manager_id == userId)
                    .ToList();
                if (visible.Count == 0) continue;

                var first = visible.First();
                var uniName = first.university?.name ?? "Вуз";
                var overdue = l.valid_until < now;

                // Договор для глубокой ссылки. Лицензия может быть
                // привязана к НЕСКОЛЬКИМ взаимодействиям разных вузов,
                // поэтому: 1) ищем первое видимое взаимодействие,
                // у которого contract_id заполнен; 2) если такого нет —
                // любое договорное взаимодействие по ЛЮБОМУ из вузов
                // лицензии. Иначе страница «Договоры» откроется без
                // подсветки (симптом «переносит, но не показывает»)
                int? contractId = visible
                    .FirstOrDefault(i => i.contract_id != null)
                    ?.contract_id;

                if (contractId == null)
                {
                    var uniIds = visible
                        .Where(i => i.university_id != null)
                        .Select(i => i.university_id)
                        .Distinct()
                        .ToList();

                    if (uniIds.Count > 0)
                    {
                        contractId = await _db.interactions
                            .Where(i =>
                                i.contract_id != null &&
                                uniIds.Contains(i.university_id))
                            .Select(i => (int?)i.contract_id)
                            .FirstOrDefaultAsync(ct);
                    }
                }

                list.Add(new NotificationDto
                {
                    Id = $"license-{l.licenses_id}",
                    Type = "license_expiring",
                    Title = overdue
                        ? "Лицензия истекла"
                        : "Лицензия истекает менее чем через 30 дней",
                    Text = $"{uniName} — лицензия действует до " +
                           $"{l.valid_until:dd.MM.yyyy}. Требуется продление.",
                    CreatedAt = l.valid_until,
                    IsRead = false,
                    Link = contractId != null
                        ? $"/contracts?focus={contractId.Value}"
                        : "/contracts",
                });
            }

            return list;
        }
    }
}