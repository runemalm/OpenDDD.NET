using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain;
using DDD.Infrastructure.Exceptions;
using DDD.Logging;

namespace DDD.Infrastructure.Ports
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

		public EventListener(
			Context context, 
			string listensTo, 
			DomainModelVersion listensToVersion, 
			TAction action, 
			IEventAdapter eventAdapter,
			IOutbox outbox,
			IDeadLetterQueue deadLetterQueue,
			ILogger logger)
		{
			Context = context;
			ListensTo = listensTo;
			ListensToVersion = listensToVersion;
			_action = action;
			_eventAdapter = eventAdapter;
			_outbox = outbox;
			_deadLetterQueue = deadLetterQueue;
			_logger = logger;
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
			
			await _deadLetterQueue.EnqueueAsync(new DeadEvent(theEvent), CancellationToken.None);
		}

		// React
		
		private TEvent MessageToEvent(IPubSubMessage message)
		{
			try
			{
				return JsonSerializer.Deserialize<TEvent>(message.ToString());
			}
			catch (Exception e)
			{
				throw new ListenerException(
					$"Couldn't translate message to event, error: '{e.Message}'",
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
