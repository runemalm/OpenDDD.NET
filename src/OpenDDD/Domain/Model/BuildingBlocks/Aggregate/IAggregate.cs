using OpenDDD.Domain.Model.BuildingBlocks.Entity;

namespace OpenDDD.Domain.Model.BuildingBlocks.Aggregate
{
	public interface IAggregate : IEntity
	{
		DomainModelVersion DomainModelVersion { get; set; }
		EntityId Id { get; }
	}
}
