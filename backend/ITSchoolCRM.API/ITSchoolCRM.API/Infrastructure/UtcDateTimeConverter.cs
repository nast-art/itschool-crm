using System.Text.Json;
using System.Text.Json.Serialization;

namespace ITSchoolCRM.API.Infrastructure;

// Колонки БД — timestamp without time zone, но по соглашению в них всегда
// лежит UTC (DateTime.UtcNow). Npgsql отдаёт их с Kind = Unspecified,
// из-за чего в JSON не попадает суффикс "Z", а фронт трактует время как
// локальное (сдвиг часового пояса теряется). Конвертер помечает такие
// значения как UTC — сериализатор добавит "Z", и браузер сам приведёт
// время к локальному поясу пользователя.
public class UtcDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return reader.GetDateTime();
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
    {
        var utc = value.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
            : value.ToUniversalTime();

        writer.WriteStringValue(utc);
    }
}