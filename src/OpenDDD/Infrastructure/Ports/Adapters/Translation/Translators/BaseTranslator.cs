using System;
using Newtonsoft.Json;
using OpenDDD.Application.Error;

namespace OpenDDD.Infrastructure.Ports.Adapters.Translation.Translators
{
    public abstract class BaseTranslator<T> : JsonConverter 
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
