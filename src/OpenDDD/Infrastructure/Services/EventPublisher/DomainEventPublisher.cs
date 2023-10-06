using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Services.Serialization;
using OpenDDD.NET.Services.MessageBrokerConnection;

namespace OpenDDD.Infrastructure.Services.EventPublisher
{
	public class DomainEventPublisher : BaseEventPublisher<IDomainEvent>, IDomainEventPublisher
	{
		public DomainEventPublisher(ILogger<DomainEventPublisher> logger, IDomainMessageBrokerConnection messageBrokerConnection, ISerializer serializer) 
			: base(logger, messageBrokerConnection, serializer)
		{
			
		}
	}
}
