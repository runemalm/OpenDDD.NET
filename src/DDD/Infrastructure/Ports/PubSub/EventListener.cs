using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DDD.Application;
using DDD.Domain.Model;
using DDD.Domain.Model.BuildingBlocks.Event;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Logging;

namespace DDD.Infrastructure.Ports.PubSub
{
	public abstract class EventListener<TEvent, TAction, TCommand> : IEventListener 
		where TEvent : IEvent
		where TAction : IAction<TCommand, object>
		where TCommand : ICommand
	{
		public Context Context { get; }
		public string ListensTo { get; }
		public DomainModelVersion ListensToVersion { get; }

		private readonly IEventAdapter _eventAdapter;
		private readonly IOutbox _outbox;
		private readonly IDeadLetterQueue _deadLetterQueue;
		private readonly TAction _action;
		private readonly ILogger _logger;
		private readonly SerializerSettings _serializerSettings;

		public EventListener(
			Context context, 
			string listensTo, 
			DomainModelVersion listensToVersion, 
			TAction action, 
			IEventAdapter eventAdapter,
			IOutbox outbox,
			IDeadLetterQueue deadLetterQueue,
			ILogger logger,
			SerializerSettings serializerSettings)
		{
			Context = context;
			ListensTo = listensTo;
			ListensToVersion = listensToVersion;
			_action = action;
			_eventAdapter = eventAdapter;
			_outbox = outbox;
			_deadLetterQueue = deadLetterQueue;
			_logger = logger;
			_serializerSettings = serializerSettings;
		}

		public Task StartAsync()
			=> _eventAdapter.SubscribeAsync(this);
		
		public Task StopAsync()
			=> _eventAdapter.UnsubscribeAsync(this);
		
		public abstract TCommand CreateCommand(TEvent theEvent);

		// Handle
		
		public async Task Handle(IPubSubMessage message)
		{
			try
			{
				await React(message);
			}
			catch (Exception e)
			{
				_logger.Log($"Action threw exception on event: {e}", LogLevel.Debug, e);
				var theEvent = MessageToEvent(message);
				theEvent.AddDeliveryFailure($"The listener threw an exception when delegating to action: {e}");
				
				var maxRetries = _eventAdapter.MaxDeliveryRetries;
				if (maxRetries == 0 || theEvent.Header.NumDeliveryRetries < maxRetries)
				{
					await AddToOutboxAsync(theEvent);
				}
				else
				{
					await DeadLetterAsync(theEvent);
				}
			}
			await _eventAdapter.AckAsync(message);
		}

		private async Task AddToOutboxAsync(IEvent theEvent)
		{
			_logger.Log(
				$"Re-adding {theEvent.Header.Name} ({theEvent.Header.DomainModelVersion}) to outbox for re-delivery to " +
				$"{_eventAdapter.GetContext()}.",
				LogLevel.Debug);
			
			await _outbox.AddAsync(theEvent.Header.ActionId, theEvent, CancellationToken.None);
		}
		
		public async Task DeadLetterAsync(IEvent theEvent)
		{
			_logger.Log(
				$"Dead lettering {theEvent.Header.Name} ({theEvent.Header.DomainModelVersion}) in " +
				$"{_eventAdapter.GetContext()}.",
				LogLevel.Debug);
			
			await _deadLetterQueue.EnqueueAsync(new DeadEvent(theEvent, _serializerSettings), CancellationToken.None);
		}

		// React
		
		private TEvent MessageToEvent(IPubSubMessage message)
		{
			try
			{
				return JsonConvert.DeserializeObject<TEvent>(message.ToString(), _serializerSettings);
			}
			catch (Exception e)
			{
				throw new ListenerException(
					$"Couldn't translate message to event {typeof(TEvent)}, error: '{e.Message}', the message: '{message.ToString()}'",
					e);
			}
		}

		public Task<bool> React(IPubSubMessage message)
			=> React(MessageToEvent(message));

		public async Task<bool> React(TEvent theEvent)
		{
			var command = CreateCommand(theEvent);
			await _action.ExecuteAsync(command, CancellationToken.None);
			return true;
		}
	}
}
