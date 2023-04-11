using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public class SerializerSettings : JsonSerializerSettings
    {
        public SerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver();
            Converters = new List<JsonConverter>()
            {
                new StringEnumConverter()
                {
                    AllowIntegerValues = false,
                    NamingStrategy = new DefaultNamingStrategy()
                },
                new ActionIdConverter(),
                new EventIdConverter(),
                new EntityIdConverter(),
                new DomainModelVersionConverter()
            };
            DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fffK";
            NullValueHandling = NullValueHandling.Ignore;
            Formatting = Formatting.Indented;
        }
    }
}
