using Newtonsoft.Json;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Infrastructure.Persistence.Serializers;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.Serializers
{
    public class OpenDddAggregateSerializer : IAggregateSerializer
    {
        private readonly JsonSerializerSettings _settings;

        public OpenDddAggregateSerializer()
        {
            _settings = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
                ContractResolver = new OpenDddPrivateSetterContractResolver(), // Enables private setters
                NullValueHandling = NullValueHandling.Include
            };
        }

        public string Serialize<TAggregate, TId>(TAggregate aggregate) 
            where TAggregate : AggregateRootBase<TId>
        {
            return JsonConvert.SerializeObject(aggregate, _settings);
        }

        public TAggregate Deserialize<TAggregate, TId>(string document) 
            where TAggregate : AggregateRootBase<TId>
        {
            return JsonConvert.DeserializeObject<TAggregate>(document, _settings)!;
        }
    }
}
