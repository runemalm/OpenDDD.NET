using Microsoft.Extensions.DependencyInjection;
using OpenDDD.Application;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Ports.MessageBroker;
using OpenDDD.Infrastructure.Services.Serialization;

namespace OpenDDD.Infrastructure.Ports.Events
{
	public abstract class BaseDomainEventListener<TEvent, TAction, TCommand, TReturns> : BaseEventListener<TEvent, TAction, TCommand, TReturns> 
		where TEvent : IDomainEvent
		where TAction : IAction<TCommand, TReturns>
		where TCommand : ICommand
	{
		public BaseDomainEventListener(IMessageBrokerConnection messageBrokerConnection, string listensTo, BaseDomainModelVersion domainModelVersion, string context, string listenerContext, ISerializer serializer, IServiceScopeFactory serviceScopeFactory) 
			: base(messageBrokerConnection, listensTo, domainModelVersion, context, listenerContext, serializer, serviceScopeFactory)
		{
			
		}
	}
}
