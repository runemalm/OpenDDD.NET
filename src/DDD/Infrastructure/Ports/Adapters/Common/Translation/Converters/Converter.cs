using System;
using Newtonsoft.Json;

namespace DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters
{
    public abstract class Converter<T> : JsonConverter<T> 
    {
        public bool CanConvert(Type type)
        {
            return type.IsSubclassOf(typeof(T));
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

            return (T)Convert.ChangeType(theObject, objectType);
        }
    }
}
