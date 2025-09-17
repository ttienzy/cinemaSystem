using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Convertors
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        private const string Format = @"hh\:mm";

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return TimeSpan.ParseExact(value, Format, null);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }

}
