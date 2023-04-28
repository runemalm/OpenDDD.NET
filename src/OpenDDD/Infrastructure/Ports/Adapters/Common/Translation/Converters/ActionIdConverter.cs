using System;
using Newtonsoft.Json;
using OpenDDD.Application;

namespace OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class ActionIdConverter : Converter<ActionId>
    {
        public override void WriteJson(
            JsonWriter writer, 
            object? value,
            JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(
            JsonReader reader, 
            Type objectType, 
            object? existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;
            return ReadJsonUsingMethod(reader, "Parse", objectType);
        }
    }
}
