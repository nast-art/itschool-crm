namespace ITSchoolCRM.API.Services.Interfaces;

/// <summary>
/// Синхронизация пользователей Keycloak в таблицу users CRM.
/// Необходима, потому что на users.users_id ссылаются внешние ключи
/// interactions.manager_id и university_managers.user_id.
/// Колонки full_name в таблице нет: полное ФИО всегда собирается
/// из частей (last_name, first_name, middle_name) на месте использования.
/// </summary>
public interface IUserSyncService
{
    /// <summary>
    /// Создаёт или обновляет запись пользователя в таблице users
    /// по keycloak_user_id (поле "sub" из JWT).
    /// </summary>
    /// <returns>Идентификатор users.users_id.</returns>
    Task<int> UpsertAsync(
        string keycloakUserId,
        string? lastName,
        string? firstName,
        string? middleName,
        string? email,
        CancellationToken cancellationToken);

    /// <summary>
    /// Создаёт запись для текущего авторизованного пользователя,
    /// если её ещё нет в таблице users. Используется в /api/Auth/me,
    /// чтобы пользователи, созданные вручную в консоли Keycloak
    /// (admin, manager1, user1...), автоматически попадали в БД.
    /// </summary>
    /// <returns>users.users_id или null, если пользователь не авторизован.</returns>
    Task<int?> EnsureCurrentUserAsync(
        CancellationToken cancellationToken);
}