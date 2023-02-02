using System;
using DDD.Application;
using Newtonsoft.Json;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class ActionIdConverter : Converter<ActionId>
    {
        public override void WriteJson(
            JsonWriter writer, 
            ActionId value, 
            JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override ActionId ReadJson(
            JsonReader reader, 
            Type objectType, 
            ActionId existingValue,
            bool hasExistingValue, 
            JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return new ActionId() { Value = s };
        }
    }
}
