using System;
using Newtonsoft.Json;
using DDD.Domain.Model.BuildingBlocks.Event;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.NewtonSoft
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
