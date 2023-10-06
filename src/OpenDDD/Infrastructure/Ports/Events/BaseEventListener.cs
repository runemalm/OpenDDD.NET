using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenDDD.Application;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Ports.MessageBroker;
using OpenDDD.Infrastructure.Services.Serialization;

namespace OpenDDD.Infrastructure.Ports.Events
{
	public abstract class BaseEventListener<TEvent, TAction, TCommand, TReturns> : Subscriber, IEventListener<TEvent, TAction, TCommand, TReturns>
		where TEvent : IEvent
		where TAction : IAction<TCommand, TReturns>
        where TCommand : ICommand
	{
		private readonly ISerializer _serializer;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		
		public BaseEventListener(IMessageBrokerConnection messageBrokerConnection, string listensTo, BaseDomainModelVersion domainModelVersion, string context, string listenerContext, ISerializer serializer, IServiceScopeFactory serviceScopeFactory) 
			: base(
				messageBrokerConnection, 
				new Topic($"{context}.{listensTo}V{domainModelVersion.Major}"), 
				new ConsumerGroup($"{context}.{listensTo}V{domainModelVersion.Major}->{listenerContext}.{typeof(TAction).Name}"))
		{
			ListensTo = listensTo;
			DomainModelVersion = domainModelVersion;
			Context = context;
			_serializer = serializer;
			_serviceScopeFactory = serviceScopeFactory;
		}

		// IEventListener

		public string ListensTo { get; set; }
		public string Context { get; set; }
		public BaseDomainModelVersion DomainModelVersion { get; set; }

		public abstract TCommand CreateCommand(TEvent theEvent);
		
		public async Task ReactAsync(TEvent theEvent)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				// Resolve a scoped service within the scope
				var action = scope.ServiceProvider.GetRequiredService<TAction>();

				var command = CreateCommand(theEvent);
				await action.ExecuteTrxAsync(command, ActionId.Create(), CancellationToken.None);
			}
		}
		
		public async Task HandleMessage(object sender, ISubscription.ReceivedEventArgs args)
		{
			var theEvent = _serializer.Deserialize(args.Message.ToString());
			if (!(theEvent is IEvent))
				throw new Exception("This should never happen, the json was not an IEvent.");
			await ReactAsync((TEvent)theEvent);
		}
		
		// IStartable

		public bool IsStarted { get; set; } = false;
		public void Start(CancellationToken ct)
		{
			IsStarted = true;
			Subscribe();
			if (Subscription == null)
				throw new Exception("This should never happen.");
			Subscription.Received += HandleMessage;
		}

		public Task StartAsync(CancellationToken ct, bool blocking = false)
		{
			Start(ct);
			return Task.CompletedTask;
		}

		public void Stop(CancellationToken ct)
		{
			IsStarted = false;
			Unsubscribe();
		}

		public Task StopAsync(CancellationToken ct, bool blocking = false)
		{
			Stop(ct);
			return Task.CompletedTask;
		}
		
		// Helpers

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
