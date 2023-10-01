using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OpenDDD.Infrastructure.Ports.Adapters.Translation.Translators;

namespace OpenDDD.Infrastructure.Services.Serialization
{
    public class SerializerSettings : ISerializerSettings
    {
        public JsonSerializerSettings JsonSerializerSettings { get; }
        
        public SerializerSettings()
        {
            JsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter()
                    {
                        AllowIntegerValues = false,
                        NamingStrategy = new DefaultNamingStrategy()
                    },
                    new ActionIdTranslator(),
                    new EventIdTranslator(),
                    new EntityIdTranslator(),
                    new DomainModelVersionTranslator()
                },
                DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fffK",
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects
            };
        }
    }
}