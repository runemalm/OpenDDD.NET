using OpenDDD.Application;
using OpenDDD.Domain.Model.Event;

namespace OpenDDD.Infrastructure.Ports.Events
{
    public interface IDomainEventListener<TEvent, TAction, TCommand, TReturns> : IEventListener<TEvent, TAction, TCommand, TReturns>
        where TEvent : IDomainEvent
        where TAction : IAction<TCommand, TReturns>
        where TCommand : ICommand
    {
        
    }
}
