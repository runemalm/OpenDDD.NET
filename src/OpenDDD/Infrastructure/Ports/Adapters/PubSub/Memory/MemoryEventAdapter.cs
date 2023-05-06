using System;
using System.Threading.Tasks;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Exceptions;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.Monitoring;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.Memory
{
	public class MemoryEventAdapter : EventAdapter<Subscription>
	{
		public MemoryEventAdapter(
			string topic,
			string client,
			int maxDeliveryRetries,
			ILogger logger,
			IMonitoringPort monitoringAdapter,
			ConversionSettings conversionSettings)
			: base(
				topic, 
				client, 
				maxDeliveryRetries,
				logger, 
				monitoringAdapter,
				conversionSettings)
		{
			
		}

		public override void Start()
		{
			IsStarted = true;
		}
		
		public override Task StartAsync()
		{
			IsStarted = true;
			return Task.CompletedTask;
		}

		public override void Stop()
		{
			IsStarted = false;
		}

		public override Task StopAsync()
		{
			Stop();
			return Task.CompletedTask;
		}

		public override Subscription Subscribe(IEventListener listener)
		{
			var subscription = new Subscription(listener);
			AddSubscription(subscription);
			return subscription;
		}

		public override Task<Subscription> SubscribeAsync(IEventListener listener)
			=> Task.FromResult(Subscribe(listener));

		public override void Unsubscribe(IEventListener listener)
		{
			var subscription = GetSubscription(listener);
			RemoveSubscription(subscription);
		}

		public override Task UnsubscribeAsync(IEventListener listener)
		{
			Unsubscribe(listener);
			return Task.CompletedTask;
		}

		public override Task AckAsync(IPubSubMessage message)
		{
			if (!(message is MemoryMessage))
			{
				throw new MemoryException(
					"Expected IPubSubMessage to be a MemoryMessage. " +
					"Something must be wrong with the implementation.");
			}

			var memoryMessage = (MemoryMessage)message;
			
			// Need no ack here since memory based event adapter will always succeed with delivery..

			return Task.CompletedTask;
		}

		public override async Task FlushAsync(OutboxEvent outboxEvent)
		{
			if (!IsStarted)
				throw new MemoryException("Can't flush event, memory event adapter is not started.");

			var message = new MemoryMessage(outboxEvent.JsonPayload);

			foreach (var sub in GetSubscriptions())
			{
				if (sub.EventName == outboxEvent.EventName)
				{
					if (sub.DomainModelVersion.ToStringWithWildcardMinorAndBuildVersions() ==
					    outboxEvent.DomainModelVersion.ToStringWithWildcardMinorAndBuildVersions())
					{
						try
						{
							await sub.Listener.Handle(message);
						}
						catch (Exception e)
						{
							_logger.Log(
								$"Memory event adapter received exception when " +
								$"delegating event to listener for reaction. " +
								$"Tried to publish on the " +
								$"'{TopicForEvent(outboxEvent.EventName, outboxEvent.DomainModelVersion)}' " +
								$"exchange.",
								LogLevel.Error,
								e);
							throw;
						}
					}
				}
			}

			await base.FlushAsync(outboxEvent);
		}
	}
}
