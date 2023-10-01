using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenDDD.Infrastructure.Services.Serialization
{
    public class Serializer : ISerializer
    {
        public ISerializerSettings Settings { get; set; }

        public Serializer(ISerializerSettings settings)
        {
            Settings = settings;
        }

        public string Serialize<T>(T input)
        {
            string serialized = JsonConvert.SerializeObject(input, Settings.JsonSerializerSettings);
            return serialized;
        }

        public T Deserialize<T>(string input)
        {
            T deserialized = JsonConvert.DeserializeObject<T>(input, Settings.JsonSerializerSettings);
            return deserialized;
        }
        
        public object Deserialize(string input)
        {
            var deserialized = JsonConvert.DeserializeObject(input, Settings.JsonSerializerSettings);
            return deserialized;
        }
        
        public string SerializePropertyName(string input)
        {
            /*
             * This is a hack.
             *
             * Current implementation is based around an intermediate dummy object that is serialized and deserialized.
             *
             * New implementation should use the property name serialization settings from the
             * JsonSerializerSettings.ContractResolver and serialize without using the dummy object solution.
             */
            var formatted = "";

            var dummyDict = 
                new Dictionary<string, string>
                {
                    { input, "dummyValue" }
                };
            var dummyJson = Serialize(dummyDict);
            var dummyObject = JObject.FromObject(Deserialize<object>(dummyJson));

            foreach (var kvp in dummyObject)
                formatted = kvp.Key;

            return formatted;
        }
    }
}
