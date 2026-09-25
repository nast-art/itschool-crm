namespace ITSchoolCRM.API.Caching;

/// <summary>
/// Типизированный доступ к распределённому кэшу (KeyDB).
/// Паттерн cache-aside: читаем кэш → при промахе вызываем
/// factory (обращение к PostgreSQL) → результат кладём в кэш.
///
/// Зарегистрирован как Singleton: держит соединение с KeyDB
/// и не зависит от scoped-контекста EF Core (factory получает
/// свои зависимости через параметр, а не через поля сервиса).
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Читает значение типа T по ключу. При промахе вызывает
    /// factory (загрузка из БД), сохраняет результат с TTL
    /// и возвращает его. Вызовы factory при одновременном промахе
    /// НЕ дедуплицируются (stampede защита не реализована сознательно:
    /// нагрузка 50 пользователей не даёт ощутимых параллельных
    /// промахов на одном ключе; при необходимости сюда же добавляется
    /// лок-объект по ключу).
    /// </summary>
    /// <typeparam name="T">Тип кэшируемого значения (десериализуется из JSON).</typeparam>
    /// <param name="key">Ключ из CacheKeys (может содержать {versionKey}).</param>
    /// <param name="ttl">Время жизни записи (из CacheOptions).</param>
    /// <param name="factory">Асинхронная загрузка значения при промахе.</param>
    /// <param name="ct">Токен отмены запроса.</param>
    Task<T> GetOrCreateAsync<T>(
        string key,
        TimeSpan ttl,
        Func<CancellationToken, Task<T>> factory,
        CancellationToken ct = default);

    /// <summary>
    /// Инвалидирует группу версионированных ключов: атомарно
    /// инкрементирует счётчик версии (INCR на KeyDB).
    /// Все ключи, собранные со старой версией, перестают находиться;
    /// физически удалятся по истечении своих TTL.
    /// </summary>
    /// <param name="versionKey">Счётчик: CacheKeys.CatalogVersion,
    /// CacheKeys.InteractionVersionKey или CacheKeys.WorkflowVersion(id).</param>
    Task BumpVersionAsync(
        string versionKey,
        CancellationToken ct = default);

    /// <summary>Удаляет один конкретный ключ из кэша.</summary>
    Task RemoveAsync(
        string key,
        CancellationToken ct = default);
}