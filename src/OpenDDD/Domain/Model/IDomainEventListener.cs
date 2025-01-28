namespace OpenDDD.Domain.Model
{
	public interface IDomainEventListener<in TEvent> : IEventListener<TEvent>
		where TEvent : IDomainEvent
	{
	}
}
