using System;
using Newtonsoft.Json;
using OpenDDD.Domain.Model.Entity;

namespace OpenDDD.Infrastructure.Ports.Adapters.Translation.Translators
{
    public class EntityIdTranslator : BaseTranslator<EntityId>
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
            return ReadJsonUsingMethod(reader, "Create", objectType);
        }
    }
}
