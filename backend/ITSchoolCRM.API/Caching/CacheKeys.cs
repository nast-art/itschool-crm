namespace ITSchoolCRM.API.Caching;

/// <summary>
/// Единая конвенция имён ключей кэша. ВСЕ ключи собираются только здесь.
/// Префикс "itschoolcrm:" добавляется автоматически (InstanceName в AddStackExchangeRedisCache).
/// </summary>
/// <remarks>
/// Версионирование: {ключ-счётчика} в строке ключа разворачивается в актуальную версию
/// (CacheService подставляет значение счётчика). Инвалидация группы = INCR счётчика,
/// SCAN/KEYS не используем — это блокирующие операции на KeyDB.
///
/// Серверы разграничивают доступ внутри выборок (UserAccessService.GetAccessibleUniversityIds),
/// поэтому ключи списков включают scope — идентификатор текущего пользователя (KeycloakUserId).
/// Общий ключ дал бы менеджеру кэш другого менеджера — утечка данных (152-ФЗ).
/// </remarks>
public static class CacheKeys
{
    // ============================================================
    // СЧЁТЧИКИ ВЕРСИЙ
    // ============================================================

    /// <summary>
    /// Версия всех справочников (вузы, направления, программы, продукты, пользователи).
    /// Импорт каталога и любая запись в справочники делают один INCR — всё сброшено разом.
    /// Справочники маленькие, дробная инвалидация им не нужна.
    /// </summary>
    public const string CatalogVersion = "catalog:version";

    /// <summary>
    /// Версия взаимодействий и всего связанного: карточки, списки, история,
    /// вложения, агрегаты статистики. Любое изменение (статус, комментарий,
    /// файл, карточка, импорт) делает INCR этого счётчика.
    /// </summary>
    public const string InteractionVersionKey = "interaction:version";

    /// <summary>Счётчик версии конкретного workflow (по id).</summary>
    public static string WorkflowVersion(int workflowId) => $"workflow:{workflowId}:version";

    // ============================================================
    // СПРАВОЧНИКИ
    // ============================================================

    public const string Universities = "catalog:universities";
    public const string Directions = "catalog:directions";
    public const string Programs = "catalog:programs";
    public const string Products = "catalog:products";
    public const string Users = "catalog:users";

    /// <summary>
    /// Версионированный ключ справочника. Результат: "catalog:universities:v{catalog:version}".
    /// </summary>
    public static string Catalog(string catalogName) => $"{catalogName}:v{{{CatalogVersion}}}";

    /// <summary>
    /// Версионированный ключ справочника с учётом scope пользователя.
    /// Нужен там, где выборка фильтруется по доступу
    /// (UniversityService.GetAllAsync/GetActiveAsync/SearchAsync режут по GetAccessibleUniversityIds).
    /// Полный доступ (manager/admin) использует scope "full" — общий кэш.
    /// </summary>
    public static string CatalogForScope(string catalogName, string scope)
        => $"{catalogName}:scope:{scope}:v{{{CatalogVersion}}}";

    /// <summary>
    /// Scope кэша текущего пользователя: "full" для manager/admin (видят всё —
    /// общий кэш уместен), иначе KeycloakUserId (у каждого менеджера своя выборка вузов).
    /// </summary>
    public const string FullAccessScope = "full";

    // ============================================================
    // WORKFLOW
    // ============================================================

    /// <summary>Схема workflow (статусы + переходы), версионированная.</summary>
    public static string Workflow(int workflowId) => $"workflow:{workflowId}:v{{{WorkflowVersion(workflowId)}}}";

    // ============================================================
    // ВЗАИМОДЕЙСТВИЯ
    // ============================================================

    /// <summary>
    /// Карточка одного взаимодействия. БЕЗ scope: доступ проверяется до обращения
    /// к кэшу (HasAccessToUniversityAsync), карточка одинакова для всех, кому разрешено её видеть.
    /// </summary>
    public static string Interaction(int interactionId) => $"interaction:{interactionId}:v{{{InteractionVersionKey}}}";

    /// <summary>
    /// Список взаимодействий — СО СКОПОМ: выборка фильтруется по
    /// university_managers, у каждого пользователя свой набор.
    /// </summary>
    public static string InteractionList(string scope) => $"interactions:scope:{scope}:v{{{InteractionVersionKey}}}";

    /// <summary>История статусов и комментариев карточки.</summary>
    public static string InteractionHistory(int interactionId) => $"interaction:{interactionId}:history:v{{{InteractionVersionKey}}}";

    /// <summary>Список вложений карточки.</summary>
    public static string Attachments(int interactionId) => $"interaction:{interactionId}:attachments:v{{{InteractionVersionKey}}}";

    // ============================================================
    // СТАТИСТИКА
    // ============================================================

    /// <summary>
    /// Агрегат статистики по фильтру. scope обязателен: статистика считается
    /// по доступному пользователю набору. filterHash — 16 hex-символов из
    /// FilterHash.Compute: каждая комбинация периода/вузов/направлений/продуктов/
    /// менеджеров — свой ключ.
    /// </summary>
    public static string Statistics(string scope, string filterHash) => $"statistics:scope:{scope}:{filterHash}";
}