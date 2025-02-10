using OpenDDD.Domain.Model.Base;

namespace OpenDDD.Infrastructure.Persistence.Serializers
{
    public interface IAggregateSerializer
    {
        string Serialize<TAggregate, TId>(TAggregate aggregate) where TAggregate : AggregateRootBase<TId>;
        TAggregate Deserialize<TAggregate, TId>(string document) where TAggregate : AggregateRootBase<TId>;
    }
}
