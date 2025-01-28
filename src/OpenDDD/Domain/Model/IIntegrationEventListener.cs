namespace OpenDDD.Domain.Model
{
	public interface IIntegrationEventListener<in TEvent> : IEventListener<TEvent>
		where TEvent : IIntegrationEvent
	{
	}
}
