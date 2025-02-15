using Newtonsoft.Json;
using OpenDDD.Infrastructure.Persistence.Serializers;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.Serializers
{
    public class OpenDddSerializer : ISerializer
    {
        protected readonly JsonSerializerSettings Settings;

        public OpenDddSerializer()
        {
            Settings = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new OpenDddPrivateSetterContractResolver(),
                NullValueHandling = NullValueHandling.Include
            };
        }

        public virtual string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }

        public virtual T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Settings)!;
        }
    }
}
