using ITSchoolCRM.API.DTOs.Responsible;

namespace ITSchoolCRM.API.Services.Interfaces;

/// <summary>
/// Раздел «Ответственные»: менеджеры школы (закрепление за вузами
/// через university_managers) и представители вузов (каталог
/// university_contests -> university_contacts).
/// Работает ТОЛЬКО на таблицах схемы заказчика, без миграций БД.
/// </summary>
public interface IResponsiblesService
{
    /// <summary>
    /// «Менеджеры школы» = пользователи, которые фигурируют как ответственные:
    /// university_managers.user_id ∪ interactions.manager_id.
    /// (Роли живут в Keycloak, отдельной колонки в users нет —
    ///  список строится из существующих назначений.)
    /// </summary>
    Task<List<ResponsibleManagerDto>> GetManagersAsync(CancellationToken ct = default);

    /// <summary>
    /// Полная замена закрепления вузов за менеджером (транзакция + аудит).
    /// </summary>
    Task<ResponsibleManagerDto> UpdateManagerUniversitiesAsync(
        int userId,
        UpdateManagerUniversitiesDto dto,
        int? actingUserId,
        CancellationToken ct = default);

    Task<List<UniversityContactDto>> GetContactsAsync(CancellationToken ct = default);

    Task<UniversityContactDto> CreateContactAsync(
        SaveUniversityContactDto dto,
        int? actingUserId,
        CancellationToken ct = default);

    Task<UniversityContactDto> UpdateContactAsync(
        int id,
        SaveUniversityContactDto dto,
        int? actingUserId,
        CancellationToken ct = default);

    /// <summary>Мягкое удаление: is_active = false (152-ФЗ, аудит).</summary>
    Task DeleteContactAsync(int id, int? actingUserId, CancellationToken ct = default);

    /// <summary>
    /// Резолв users.users_id по keycloak_user_id (claim "sub") —
    /// для audit_logs.user_id. Вынесен в сервис,
    /// чтобы контроллер не трогал DbContext.
    /// </summary>
    Task<int?> ResolveUserIdByKeycloakSubAsync(string keycloakSub, CancellationToken ct = default);
}