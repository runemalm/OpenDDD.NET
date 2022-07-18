namespace DDD.Domain
{
	public interface IAggregate : IEntity
	{
		EntityId Id { get; }
	}
}
