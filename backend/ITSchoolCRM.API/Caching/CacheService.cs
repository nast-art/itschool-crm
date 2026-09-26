using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Caching.Distributed;

using StackExchange.Redis;

namespace ITSchoolCRM.API.Caching;

/// <summary>
/// Реализация ICacheService поверх IDistributedCache (KeyDB/Redis)
/// и IConnectionMultiplexer (для атомарного INCR версий).
/// </summary>
/// <remarks>
/// Сериализация — System.Text.Json с camelCase: формат совпадает
/// с тем, что фронтенд ожидает от контроллеров, и с jsonb-аудитом.
///
/// ОТКАЗОУСТОЙЧИВОСТЬ (все обращения к KeyDB — «best effort»):
/// кэш НЕ является единой точкой отказа. При недоступности KeyDB
/// каждый метод перехватывает исключение, пишет warning в лог и
/// обслуживает запрос напрямую из источника (PostgreSQL).
/// Система деградирует по скорости, но НЕ по доступности.
/// Для BumpVersionAsync это тем более безопасно: инвалидация
/// best-effort — TTL всё равно обновит протухшие записи.
/// </remarks>
public sealed class CacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        // camelCase — как в DTO-контракте с фронтендом
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // null-поля не пишем: короче payload,
        // десериализация вернёт null и без них
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IDistributedCache cache, IConnectionMultiplexer multiplexer, ILogger<CacheService> logger)
    {
        _cache = cache;
        _multiplexer = multiplexer;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<CancellationToken, Task<T>> factory, CancellationToken ct = default)
    {
        // Шаг 1: разрешение версионных плейсхолдеров
        // ("interaction:5:v{interaction:version}" -> "...v7").
        // Если KeyDB недоступна — обслуживаем запрос без кэша.
        string resolvedKey;
        try
        {
            resolvedKey = await ResolveVersionsAsync(key, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "KeyDB недоступна при разрешении версии ключа {Key}. " +
                "Запрос обслуживается без кэша.", key);

            return await factory(ct);
        }

        // Шаг 2: чтение из кэша. Сбой чтения трактуем как промах —
        // данные возьмём из источника, запрос не прерываем.
        string? cached = null;

        try
        {
            cached = await _cache.GetStringAsync(resolvedKey, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "KeyDB недоступна при чтении ключа {Key}. " +
                "Запрос обслуживается без кэша.", resolvedKey);
        }

        if (cached is not null)
        {
            _logger.LogTrace("Cache HIT: {Key}", resolvedKey);

            return JsonSerializer.Deserialize<T>(cached, JsonOptions)!;
        }

        _logger.LogTrace("Cache MISS: {Key}", resolvedKey);

        // Шаг 3: промах — загружаем из источника (PostgreSQL через EF Core).
        // Factory вызываем ДО записи в кэш: если источник бросил исключение,
        // в кэш не попадёт «пустышка». Исключение источника НЕ перехватываем —
        // это реальный сбой, его видит глобальный обработчик (ERR_INTERNAL).
        var value = await factory(ct);

        // Шаг 4: запись в кэш — best effort. Неудача записи не ломает запрос:
        // данные уже получены из источника, следующий запрос просто снова промахнётся.
        try
        {
            var payload = JsonSerializer.Serialize(value, JsonOptions);

            await _cache.SetStringAsync(resolvedKey, payload,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "KeyDB недоступна при записи ключа {Key}. " +
                "Результат возвращён без кэширования.", resolvedKey);
        }

        return value;
    }

    /// <inheritdoc />
    public async Task BumpVersionAsync(string versionKey, CancellationToken ct = default)
    {
        // Инвалидация — best effort: если KeyDB лежит, TTL всё равно
        // обновит протухшие записи, а прерывать запись из-за сброса кэша недопустимо.
        try
        {
            // INCR атомарен на KeyDB — при параллельных записях
            // из разных реплик API счётчик не потеряет инкрементов
            var db = _multiplexer.GetDatabase();

            var newVersion = await db.StringIncrementAsync(versionKey);

            _logger.LogDebug("Сбой в версии кэша: {VersionKey} -> v{Version}", versionKey, newVersion);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "KeyDB недоступна при инвалидации версии {VersionKey}. " +
                "Инвалидация пропущена (TTL обновит записи).", versionKey);
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        // Удаление отдельного ключа — тоже best effort:
        // не удалился сейчас, удалится по TTL.
        try
        {
            await _cache.RemoveAsync(key, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "KeyDB недоступна при удалении ключа {Key}.", key);
        }
    }

    /// <summary>
    /// Заменяет в ключе каждый шаблон {versionKey} на актуальное
    /// значение соответствующего счётчика. Отсутствующий счётчик
    /// трактуем как версию "1" — первое обращение само его создаст
    /// при первом BumpVersionAsync.
    /// </summary>
    /// <example>
    /// "interaction:5:v{interaction:version}" при счётчике 7
    /// превращается в "interaction:5:v7".
    /// </example>
    /// <remarks>
    /// Метод НЕ перехватывает исключения KeyDB — это делает
    /// вызывающий GetOrCreateAsync, чтобы решить о fallback.
    /// </remarks>
    private async Task<string> ResolveVersionsAsync(string key, CancellationToken ct)
    {
        var result = key;
        var db = _multiplexer.GetDatabase();

        while (true)
        {
            var start = result.IndexOf('{', StringComparison.Ordinal);

            if (start < 0)
            {
                break;
            }

            var end = result.IndexOf('}', start);

            if (end < 0)
            {
                // Незакрытая скобка — некорректный ключ, лучше
                // промах кэша, чем загадочное поведение
                _logger.LogWarning("Незакрытый заполнитель версии в ключе кэша: {Key}", key);
                break;
            }

            var versionKey = result.Substring(start + 1, end - start - 1);
            var version = await db.StringGetAsync(versionKey);
            var versionValue = version.HasValue? (string)version! : "1";

            result = result.Remove(start, end - start + 1).Insert(start, versionValue);
        }
        return result;
    }
}