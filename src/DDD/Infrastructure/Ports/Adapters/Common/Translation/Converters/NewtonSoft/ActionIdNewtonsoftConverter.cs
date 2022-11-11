using System;
using Newtonsoft.Json;
using DDD.Application;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.NewtonSoft
{
    public class ActionIdNewtonsoftConverter : JsonConverter<ActionId>
    {
        public override void WriteJson(JsonWriter writer, ActionId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override ActionId ReadJson(JsonReader reader, Type objectType, ActionId existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return new ActionId() { Value = s };
        }
    }
}
