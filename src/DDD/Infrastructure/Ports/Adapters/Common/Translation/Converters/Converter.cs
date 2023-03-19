using System;
using Newtonsoft.Json;
using DDD.Application.Error;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public abstract class Converter<T> : JsonConverter 
    {
        public override bool CanConvert(Type objectType)
        {
            var canConvert = objectType.IsSubclassOf(typeof(T)) || objectType == typeof(T);
            return canConvert;
        }
        
        public override void WriteJson(
            JsonWriter writer, 
            object? value,
            JsonSerializer serializer)
        {
            if (value == null)
                throw new DddException("Should never be here! TODO: Handle this exception better.");
            writer.WriteValue(value.ToString());
        }

        public T ReadJsonUsingMethod(
            JsonReader reader, 
            string methodName, 
            Type objectType)
        {
            var value = (string)reader.Value;
            
            var theObject =
                objectType
                    .GetMethod(methodName, new[] { typeof(string) })
                    .Invoke(null, new object[] { value });
            
            return (T)theObject;
        }
    }
}
