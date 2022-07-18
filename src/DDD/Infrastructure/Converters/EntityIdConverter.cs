using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DDD.Domain;
using DDD.Application.Exceptions;

namespace DDD.Infrastructure.Converters
{
    public class EntityIdConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsSubclassOf(typeof(EntityId));
        }
        
        public override JsonConverter CreateConverter(
            Type type,
            JsonSerializerOptions options)
        {
            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(EntityIdConverterInner<>).MakeGenericType(
                    new Type[] { type }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: new object[] { options },
                culture: null)!;
    
            return converter;
        }
        
        private class EntityIdConverterInner<T> : JsonConverter<T> where T : EntityId
        {
            public EntityIdConverterInner(JsonSerializerOptions options)
            {
                
            }
        
            public override T Read(
                ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {
                var domainModelVersion = DomainModelVersion.Latest();
                var value = reader.GetString();
                
                var entityId = 
                    typeToConvert.GetMethod("Create", new [] {typeof(DomainModelVersion), typeof(string)})
                        .Invoke(null, new object[] { domainModelVersion, value });
                
                return (T)entityId;
                
                
                // if (reader.TokenType != JsonTokenType.StartObject)
                // {
                //     throw new JsonException();
                // }
                //
                // string domainModelVersionString = "";
                // string value = "";
                //
                // while (reader.Read())
                // {
                //     if (reader.TokenType == JsonTokenType.EndObject)
                //     {
                //         var domainModelVersion = DomainModelVersion.Create(domainModelVersionString);
                //         var entityId = 
                //             typeToConvert.GetMethod("Create", new [] {typeof(DomainModelVersion), typeof(string)})
                //                 .Invoke(null, new object[] { domainModelVersion, value });
                //
                //         return (T)entityId;
                //     }
                //
                //     // Get the key.
                //     if (reader.TokenType != JsonTokenType.PropertyName)
                //     {
                //         throw new JsonException();
                //     }
                //
                //     string? propertyName = reader.GetString();
                //
                //     reader.Read();
                //
                //     if (propertyName == propertyNameAccordingToNamingPolicy("Value", options))
                //         value = reader.GetString();
                //     else if (propertyName == propertyNameAccordingToNamingPolicy("DomainModelVersion", options))
                //         domainModelVersionString = reader.GetString();
                //     else
                //         throw new DddException(
                //             $"Unexpected property name in JSON string when trying to convert EntityId: '{propertyName}'");
                // }
                //
                // throw new JsonException();
            }
        
            public override void Write(
                Utf8JsonWriter writer,
                T entityId,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(entityId.Value);
                
                // writer.WriteStartObject();
                //
                // writer.WriteString(propertyNameAccordingToNamingPolicy("Value", options), entityId.Value);
                // writer.WriteString(propertyNameAccordingToNamingPolicy("DomainModelVersion", options), entityId.DomainModelVersion.ToString());
                //
                // writer.WriteEndObject();
            }

            private string propertyNameAccordingToNamingPolicy(string input, JsonSerializerOptions options)
            {
                if (String.IsNullOrEmpty(input))
                    return input;

                var isCamelCase = options.PropertyNamingPolicy == JsonNamingPolicy.CamelCase;
                
                if (isCamelCase)
                    return input.First().ToString().ToLower() + input.Substring(1);
                
                return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
    }
}