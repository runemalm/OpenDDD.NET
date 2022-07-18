using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain;
using DDD.Infrastructure.Exceptions;

namespace DDD.Infrastructure.Ports
{
	public abstract class EventListener<TEvent, TAction, TCommand> : IEventListener 
		where TEvent : IEvent
		where TAction : IAction<TCommand, object>
		where TCommand : ICommand
	{
		public Context Context { get; }
		public string ListensTo { get; }
		public IDomainModelVersion ListensToVersion { get; }

		private readonly IEventAdapter _eventAdapter;
		private readonly TAction _action;

		public EventListener(Context context, string listensTo, IDomainModelVersion listensToVersion, TAction action, IEventAdapter eventAdapter)
		{
			Context = context;
			ListensTo = listensTo;
			ListensToVersion = listensToVersion;
			_action = action;
			_eventAdapter = eventAdapter;
		}

		public Task Start()
			=> _eventAdapter.SubscribeAsync(this);
		
		public abstract TCommand CreateCommand(TEvent theEvent);

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
