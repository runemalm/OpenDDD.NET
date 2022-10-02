namespace DDD.Domain
{
	public interface IAggregate : IEntity
	{
		DomainModelVersion DomainModelVersion { get; set; }
		EntityId Id { get; }
	}
}
