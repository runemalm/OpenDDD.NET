using System;
using DDD.Domain;
using Newtonsoft.Json;
using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace DDD.Infrastructure.Converters.NewtonSoft
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


    // public class EntityIdNewtonsoftConverter : JsonConverter<EntityId>
    // {
    //     
    //     
    //     public override void WriteJson(JsonWriter writer, EntityId value, JsonSerializer serializer)
    //     {
    //         writer.WriteValue(value.ToString());
    //     }
    //
    //     public override EntityId ReadJson(JsonReader reader, Type objectType, EntityId existingValue,
    //         bool hasExistingValue, JsonSerializer serializer)
    //     {
    //         string s = (string)reader.Value;
    //         throw new NotImplementedException();
    //         // return EntityId.Create(s);
    //     }
    // }
}



// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text.Json;
// using DDD.Application.Exceptions;
// using DDD.Domain;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Linq;
// using JsonException = Newtonsoft.Json.JsonException;
// using JsonSerializer = Newtonsoft.Json.JsonSerializer;
// using SystemJsonSerializer = System.Text.Json.JsonSerializer;
//
// namespace DDD.Infrastructure.Converters.NewtonSoft
// {
//     public class EntityIdNewtonsoftConverter : JsonConverter
//     {
//         public override bool CanConvert(Type type)
//         {
//             // return type == typeof(EntityId);
//             return type.IsSubclassOf(typeof(EntityId));
//         }
//
//         public override object ReadJson(
//             JsonReader reader,
//             Type type,
//             object value,
//             JsonSerializer serializer)
//         {
//             throw new NotImplementedException();
//             
//             
//             
//             
//             
//             //
//             // if (reader.TokenType != JsonTokenType.StartObject)
//             // {
//             //     throw new JsonException();
//             // }
//             //     
//             // string domainModelVersionString = "";
//             // string value = "";
//             //     
//             // while (reader.Read())
//             // {
//             //     if (reader.TokenType == JsonTokenType.EndObject)
//             //     {
//             //         var domainModelVersion = DomainModelVersion.Create(domainModelVersionString);
//             //         var entityId = 
//             //             typeToConvert.GetMethod("Create", new [] {typeof(DomainModelVersion), typeof(string)})
//             //                 .Invoke(null, new object[] { domainModelVersion, value });
//             //     
//             //         return (T)entityId;
//             //     }
//             //     
//             //     // Get the key.
//             //     if (reader.TokenType != JsonTokenType.PropertyName)
//             //     {
//             //         throw new JsonException();
//             //     }
//             //     
//             //     string? propertyName = reader.GetString();
//             //     
//             //     reader.Read();
//             //
//             //     if (propertyName == propertyNameAccordingToNamingPolicy("Value", options))
//             //         value = reader.GetString();
//             //     else if (propertyName == propertyNameAccordingToNamingPolicy("DomainModelVersion", options))
//             //         domainModelVersionString = reader.GetString();
//             //     else
//             //         throw new DddException(
//             //             $"Unexpected property name in JSON string when trying to convert EntityId: '{propertyName}'");
//             // }
//             //     
//             // throw new JsonException();
//         }
//             // => SystemJsonSerializer.Deserialize(reader.ReadAsString(), type);
//             // => new EntityId(DomainModelVersion.Latest(), reader.ReadAsString());
//
//         public override void WriteJson(
//             JsonWriter writer,
//             object value,
//             JsonSerializer serializer)
//         {
//             throw new NotImplementedException();
//             
//             JToken t = JToken.FromObject(value);
//
//             if (t.Type != JTokenType.Object)
//             {
//                 throw new DddException("Expected to be a JObject..");
//                 // t.WriteTo(writer);
//             }
//             else
//             {
//                 JObject o = (JObject)t;
//                 IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();
//
//                 o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));
//
//                 o.WriteTo(writer);
//             }
//         }
//             // => SystemJsonSerializer.Serialize(value);
//         // {
//         //     var entityId = (EntityId)value;
//         //     writer.WriteValue(entityId.Value);
//         // }
//     }
// }
