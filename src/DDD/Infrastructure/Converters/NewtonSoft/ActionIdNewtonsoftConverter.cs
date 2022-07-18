using System;
using DDD.Domain;
using Newtonsoft.Json;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace DDD.Infrastructure.Converters.NewtonSoft
{
    public class ActionIdNewtonsoftConverter : JsonConverter<ActionId>
    {
        public override void WriteJson(JsonWriter writer, ActionId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override ActionId ReadJson(JsonReader reader, Type objectType, ActionId existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            return new ActionId() { Value = s };
        }
    }
}


// using System;
// using DDD.Domain;
// using Newtonsoft.Json;
//
// namespace DDD.Infrastructure.Converters.NewtonSoft
// {
//     public class ActionIdNewtonsoftConverter : JsonConverter
//     {
//         public override bool CanConvert(Type type)
//         {
//             return type == typeof(ActionId);
//         }
//  
//         public override object ReadJson(
//             JsonReader reader,
//             Type type,
//             object value,
//             JsonSerializer serializer)
//             => new ActionId() { Value = reader.ReadAsString() };
//
//         public override void WriteJson(
//             JsonWriter writer,
//             object value,
//             JsonSerializer serializer)
//         {
//             var actionId = (ActionId)value;
//             writer.WriteValue(actionId.Value);
//         }
//     }
// }
