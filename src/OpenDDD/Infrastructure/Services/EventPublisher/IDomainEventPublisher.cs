using OpenDDD.Domain.Model.Event;

namespace OpenDDD.Infrastructure.Services.EventPublisher
{
	public interface IDomainEventPublisher : IEventPublisher<IDomainEvent>
	{
		
	}
}
