using OpenDDD.Domain.Model.Base;
using OpenDDD.Infrastructure.Persistence.Serializers;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.Serializers
{
    public class OpenDddAggregateSerializer : OpenDddSerializer, IAggregateSerializer
    {
        public string Serialize<TAggregate, TId>(TAggregate aggregate)
            where TAggregate : AggregateRootBase<TId>
        {
            return Serialize<TAggregate>(aggregate);
        }

        public TAggregate Deserialize<TAggregate, TId>(string document)
            where TAggregate : AggregateRootBase<TId>
        {
            return Deserialize<TAggregate>(document);
        }
    }
}