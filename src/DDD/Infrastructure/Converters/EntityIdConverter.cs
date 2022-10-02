using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DDD.Domain;

namespace DDD.Infrastructure.Converters
{
    public class EntityIdConverter : JsonConverterFactory
    {
        public EntityIdConverter()
        {
            
        }
        
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
                var value = reader.GetString();
                
                var entityId = 
                    typeToConvert.GetMethod("Create", new [] {typeof(string)})
                        .Invoke(null, new object[] { value });
                
                return (T)entityId;
            }
        
            public override void Write(
                Utf8JsonWriter writer,
                T entityId,
                JsonSerializerOptions options)
            {
                writer.WriteStringValue(entityId.Value);
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