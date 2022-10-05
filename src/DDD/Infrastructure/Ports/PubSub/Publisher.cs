﻿using System.Collections.Generic;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain;
using DDD.Domain.Model;
using DDD.Domain.Model.BuildingBlocks;
using DDD.Domain.Model.BuildingBlocks.Event;
using DDD.Logging;

namespace DDD.Infrastructure.Ports.PubSub
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
				$"Publishing {theEvent.Header.Name} ({theEvent.Header.DomainModelVersion}) to " +
                $"{_eventAdapter.GetContext()}.",
				LogLevel.Information);
			await _eventAdapter.PublishAsync(theEvent);
		}

		public async Task FlushAsync(OutboxEvent outboxEvent)
		{
			var prefix = outboxEvent.NumDeliveryFailures > 0 ? "Re-f" : "F";
			var message = 
				$"{prefix}lushing {outboxEvent.EventName} ({outboxEvent.DomainModelVersion}) " +
				$"to {_eventAdapter.GetContext()}.";
			_logger.Log(
				message,
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