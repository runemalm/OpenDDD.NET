using System;
using DDD.Domain.Model;
using Newtonsoft.Json;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class DomainModelVersionConverter : Converter<DomainModelVersion>
    {
        public override void WriteJson(
            JsonWriter writer, 
            DomainModelVersion value, 
            JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override DomainModelVersion ReadJson(
            JsonReader reader, 
            Type objectType, 
            DomainModelVersion existingValue,
            bool hasExistingValue, 
            JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return new DomainModelVersion(s);
        }
    }
}
