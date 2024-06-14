using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpotMate.Web.Converters;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private const string TimeZoneString = "[UTC]";

    
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utcTimeString = value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        writer.WriteStringValue(utcTimeString + TimeZoneString);
    }
}