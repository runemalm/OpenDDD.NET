using System;
using Newtonsoft.Json;
using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.Ports.Adapters.Translation.Translators
{
    public class DomainModelVersionTranslator : BaseTranslator<BaseDomainModelVersion>
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
            string s = (string)reader.Value;
            return new BaseDomainModelVersion(s);
        }
    }
}
