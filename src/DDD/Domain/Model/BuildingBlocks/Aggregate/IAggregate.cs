using DDD.Domain.Model.BuildingBlocks.Entity;

namespace DDD.Domain.Model.BuildingBlocks.Aggregate
{
	public interface IAggregate : IEntity
	{
		DomainModelVersion DomainModelVersion { get; set; }
		EntityId Id { get; }
	}
}
