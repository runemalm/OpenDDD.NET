using OpenDDD.Domain.Model.Entity;

namespace OpenDDD.Domain.Model.AggregateRoot
{
	public interface IAggregateRoot : IEntity
	{
		BaseDomainModelVersion DomainModelVersion { get; set; }
		EntityId Id { get; }
	}
}
