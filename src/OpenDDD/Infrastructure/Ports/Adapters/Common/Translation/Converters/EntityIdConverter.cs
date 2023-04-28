using System;
using Newtonsoft.Json;
using OpenDDD.Domain.Model.BuildingBlocks.Entity;

namespace OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class EntityIdConverter : Converter<EntityId>
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
