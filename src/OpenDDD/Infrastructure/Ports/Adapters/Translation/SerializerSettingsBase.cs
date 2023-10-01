// using System.Collections.Generic;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Converters;
// using Newtonsoft.Json.Serialization;
// using OpenDDD.Infrastructure.Ports.Adapters.Translation.Translators;
//
// namespace OpenDDD.Infrastructure.Ports.Adapters.Translation
// {
//     public class SerializerSettingsBase : JsonSerializerSettings, ISerializerSettings
//     {
//         public SerializerSettingsBase()
//         {
//             ContractResolver = new CamelCasePropertyNamesContractResolver();
//             Converters = new List<JsonConverter>()
//             {
//                 new StringEnumConverter()
//                 {
//                     AllowIntegerValues = false,
//                     NamingStrategy = new DefaultNamingStrategy()
//                 },
//                 new ActionIdTranslator(),
//                 new EventIdTranslator(),
//                 new EntityIdTranslator(),
//                 new DomainModelVersionTranslator()
//             };
//             DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fffK";
//             NullValueHandling = NullValueHandling.Ignore;
//             Formatting = Formatting.Indented;
//         }
//     }
// }
