using System;
using Newtonsoft.Json;
using DDD.Domain.Model.BuildingBlocks.Entity;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.NewtonSoft
{
    public class EntityIdNewtonsoftConverter : JsonConverter
    {
        public override bool CanConvert(Type type)
        {
            return type.IsSubclassOf(typeof(EntityId));
        }

        public override void WriteJson(
            JsonWriter writer, 
            object value, 
            JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(
            JsonReader reader, 
            Type objectType, 
            object existingValue, 
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
