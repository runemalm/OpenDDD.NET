using System.Collections.Generic;
using System.Threading.Tasks;
using DDD.Domain;
using DDD.Logging;

namespace DDD.Infrastructure.Ports
{
	public class Publisher : IPublisher
	{
		private IEventAdapter _eventAdapter;
		private ILogger _logger;

		public Publisher(IEventAdapter eventAdapter, ILogger logger)
		{
			_eventAdapter = eventAdapter;
			_logger = logger;
		}

		public async Task PublishAsync(IEvent theEvent)
		{
			_logger.Log(
				$"Publishing {theEvent.EventName} ({theEvent.DomainModelVersion}) to " +
                $"{_eventAdapter.GetContext()}.",
				LogLevel.Information);
			await _eventAdapter.PublishAsync(theEvent);
		}

		public async Task FlushAsync(OutboxEvent outboxEvent)
		{
			_logger.Log(
				$"Flushing {outboxEvent.EventName} ({outboxEvent.DomainModelVersion}) to " +
				$"{_eventAdapter.GetContext()}.",
				LogLevel.Debug);
			await _eventAdapter.FlushAsync(outboxEvent);
		}

		public bool HasPublished(IEvent theEvent)
			=> _eventAdapter.HasPublished(theEvent);
		
		public bool HasFlushed(IEvent theEvent)
			=> _eventAdapter.HasFlushed(theEvent);

		public Task<IEnumerable<IEvent>> GetPublishedAsync(ActionId actionId)
			=> _eventAdapter.GetPublishedAsync(actionId);
	}
}
