using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ITSchoolCRM.API.Caching;

/// <summary>
/// Стабильный короткий хэш тела фильтра (ReportFilterDto и аналоги).
///
/// Назначение: агрегаты статистики кэшируются по комбинации
/// фильтров. Один и тот же фильтр → один и тот же ключ → попадание
/// в кэш; изменение любого поля фильтра → другой ключ → новый
/// расчёт. Хэш усечён до 16 hex-символов (64 бита): коллизия
/// двух разных фильтров при рабочих объёмах (десятки комбинаций
/// на пользователя) практически невозможна, а длина ключа остаётся
/// разумной.
///
/// Сериализация детерминирована: одинаковые значения полей дают
/// одинаковый JSON (порядок полей класса фиксирован компилятором,
/// DefaultIgnoreCondition исключает null-поля, поэтому
/// фильтр { dateFrom: null } и фильтр {} дают один хэш —
/// это корректно: semantically они эквивалентны).
/// </summary>
public static class FilterHash
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Вычисляет 16-символьный hex-хэш (первые 64 бита SHA-256)
    /// канонической JSON-сериализации фильтра.
    /// </summary>
    public static string Compute(object filter)
    {
        var json = JsonSerializer.Serialize(
            filter,
            Options);

        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(json));

        // Convert.ToHexString даёт верхний регистр; приводим
        // к нижнему для единообразия ключей
        return Convert
            .ToHexString(bytes)[..16]
            .ToLowerInvariant();
    }
}