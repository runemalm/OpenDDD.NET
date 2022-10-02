using System;
using DDD.Domain;
using Newtonsoft.Json;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace DDD.Infrastructure.Converters.NewtonSoft
{
    public class EventIdNewtonsoftConverter : JsonConverter<EventId>
    {
        public override void WriteJson(JsonWriter writer, EventId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override EventId ReadJson(JsonReader reader, Type objectType, EventId existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return new EventId() { Value = s };
        }
    }
}
