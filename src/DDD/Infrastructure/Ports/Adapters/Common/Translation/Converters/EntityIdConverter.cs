using System;
using DDD.Domain.Model.BuildingBlocks.Entity;
using Newtonsoft.Json;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class EntityIdConverter : Converter<EntityId>
    {
        public override void WriteJson(
            JsonWriter writer, 
            EntityId value,
            JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override EntityId ReadJson(
            JsonReader reader, 
            Type objectType, 
            EntityId existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;
            return ReadJsonUsingMethod(reader, "Create", objectType);
        }
    }
}
