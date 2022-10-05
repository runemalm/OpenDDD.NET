using System;
using DDD.Domain;
using DDD.Domain.Model;
using Newtonsoft.Json;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.NewtonSoft
{
    public class DomainModelVersionNewtonsoftConverter : JsonConverter<DomainModelVersion>
    {
        public override void WriteJson(JsonWriter writer, DomainModelVersion value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override DomainModelVersion ReadJson(JsonReader reader, Type objectType, DomainModelVersion existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return new DomainModelVersion(s);
        }
    }
}
