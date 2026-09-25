using ITSchoolCRM.API.Caching;
using ITSchoolCRM.API.Data;
using ITSchoolCRM.API.Models;
using ITSchoolCRM.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ITSchoolCRM.API.Services.Implementations;

/// <summary>
/// Реализация синхронизации пользователей Keycloak -> users CRM.
///
/// Логика:
///   1. Ищем пользователя в users по keycloak_user_id.
///   2. Не нашли — создаём (части ФИО, email, is_active = true).
///   3. Нашли — обновляем непустые части ФИО и email.
///
/// Колонки full_name в таблице нет. Полное ФИО собирается из частей
/// на месте использования (отчёты, списки, сопоставление при импорте).
///
/// При синхронизации из токена (EnsureCurrentUserAsync) части ФИО
/// восстанавливаем из claim "name": первое слово -> first_name,
/// последнее слово -> last_name, слова между ними -> middle_name
/// (в токене их не будет, т.к. Keycloak хранит только имя и фамилию;
/// middle_name появляется у пользователей, зарегистрированных через форму).
///
/// КЭШИРОВАНИЕ: сервис — единственная точка записи в таблицу users,
/// поэтому инвалидация кэша справочника живёт здесь (UserService
/// только читает). Инвалидируются оба счётчика:
///   - catalog:version — справочник пользователей (UserService.GetAllAsync);
///   - interaction:version — ФИО проецируется в InteractionDto.ManagerName,
///     ReportRowDto.ResponsibleName и агрегаты статистики.
/// ИНВАЛИДАЦИЯ ТОЛЬКО ПРИ РЕАЛЬНОМ ИЗМЕНЕНИИ (флаг userChanged):
/// UpsertAsync вызывается при каждом входе пользователя, а без флага
/// каждый логин сбрасывал бы весь кэш взаимодействий впустую.
/// </summary>
public class UserSyncService : IUserSyncService
{
    private readonly CrmDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UserSyncService(
        CrmDbContext context,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _context = context;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<int> UpsertAsync(
        string keycloakUserId,
        string? lastName,
        string? firstName,
        string? middleName,
        string? email,
        CancellationToken cancellationToken)
    {
        var user = await _context.users
            .FirstOrDefaultAsync(
                x => x.keycloak_user_id == keycloakUserId,
                cancellationToken);

        // Флаг «запись реально изменила данные». Первый вход
        // пользователя (user == null) — всегда изменение:
        // в справочнике появился новый человек. Без флага каждый
        // вход (Upsert вызывается при каждой сессии) делал бы
        // два INCR и сбрасывал кэш взаимодействий впустую.
        var userChanged = user is null;

        if (user is null)
        {
            user = new user
            {
                keycloak_user_id = keycloakUserId,
                last_name = lastName,
                first_name = firstName,
                middle_name = middleName,
                email = email,
                is_active = true,
                created_at = DateTime.UtcNow
            };

            _context.users.Add(user);
        }
        else
        {
            // Обновляем только непустые части и только если
            // значение действительно отличается — иначе флаг
            // не взводится и кэш не трогаем.
            if (!string.IsNullOrWhiteSpace(lastName) &&
                user.last_name != lastName)
            {
                user.last_name = lastName;
                userChanged = true;
            }

            if (!string.IsNullOrWhiteSpace(firstName) &&
                user.first_name != firstName)
            {
                user.first_name = firstName;
                userChanged = true;
            }

            if (!string.IsNullOrWhiteSpace(middleName) &&
                user.middle_name != middleName)
            {
                user.middle_name = middleName;
                userChanged = true;
            }

            if (!string.IsNullOrWhiteSpace(email) &&
                user.email != email)
            {
                user.email = email;
                userChanged = true;
            }

            if (userChanged)
            {
                user.updated_at = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        // НОВОЕ (кэш): инвалидация только при реальном изменении.
        // Новый пользователь должен появиться в фильтре
        // «Ответственный» и стать доступным для назначения
        // менеджером немедленно (без ожидания TTL); смена ФИО
        // должна отразиться в списках взаимодействий и отчётах.
        if (userChanged)
        {
            await _cache.BumpVersionAsync(
                CacheKeys.CatalogVersion,
                cancellationToken);

            await _cache.BumpVersionAsync(
                CacheKeys.InteractionVersionKey,
                cancellationToken);
        }

        return user.users_id;
    }

    public async Task<int?> EnsureCurrentUserAsync(
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated ||
            string.IsNullOrWhiteSpace(_currentUser.KeycloakUserId))
        {
            return null;
        }

        // Полное имя из claim "name" (Keycloak кладёт его через full-name
        // маппер профиля). Формат: "имя фамилия" (отчество в Keycloak
        // не хранится).
        var nameFromClaim = _currentUser.FullName;

        string? lastName = null;
        string? firstName = null;
        string? middleName = null;

        if (!string.IsNullOrWhiteSpace(nameFromClaim))
        {
            // Keycloak: "Иван Иванов" -> first_name = Иван, last_name = Иванов
            var parts = nameFromClaim.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
            {
                firstName = parts[0];
            }
            else
            {
                firstName = parts[0];
                lastName = parts[^1];

                if (parts.Length > 2)
                {
                    middleName = string.Join(' ', parts[1..^1]);
                }
            }
        }

        // Upsert сам решит, было ли изменение: при обычном входе
        // (данные те же) флаг userChanged не взведётся, INCR
        // не выполнится, кэш останется нетронутым. При первом
        // входе или смене профиля в Keycloak — инвалидирует.
        return await UpsertAsync(
            _currentUser.KeycloakUserId,
            lastName,
            firstName,
            middleName,
            _currentUser.Email,
            cancellationToken);
    }
}