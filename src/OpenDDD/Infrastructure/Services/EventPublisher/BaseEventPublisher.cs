using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Ports.MessageBroker;
using OpenDDD.Infrastructure.Services.Serialization;

namespace OpenDDD.Infrastructure.Services.EventPublisher
{
	public abstract class BaseEventPublisher<T> : IEventPublisher<T> 
		where T : IEvent
	{
		private ILogger _logger;
		protected IMessageBrokerConnection _messageBrokerConnection;
		protected ISerializer _serializer;
		
		public BaseEventPublisher(ILogger logger, IMessageBrokerConnection messageBrokerConnection, ISerializer serializer)
		{
			_logger = logger;
			_messageBrokerConnection = messageBrokerConnection;
			_serializer = serializer;
		}

		public async Task PublishAsync(T theEvent)
		{
			_logger.LogInformation($"Actually publishing {theEvent.Header.Name} ({theEvent.Header.DomainModelVersion})");
			var message = EventToMessage(theEvent);
			var topic = TopicForEvent(theEvent);
			await PublishAsync(message, topic);
		}
		
		public async Task PublishAsync(IMessage message, Topic topic)
		{
			_logger.LogInformation($"Actually publishing message to topic {topic}.");
			await _messageBrokerConnection.PublishAsync(message, topic);
		}
		
		// Helpers

		private IMessage EventToMessage(IEvent theEvent)
		{
			return new Message(_serializer.Serialize(theEvent));
		}
		
		private Topic TopicForEvent(IEvent theEvent)
		{
			var value = string.Format("{0}.{1}V{2}", 
				theEvent.Header.Context, 
				theEvent.Header.Name,
				theEvent.Header.DomainModelVersion.Major);
			return new Topic(value);
		}
	}
}
