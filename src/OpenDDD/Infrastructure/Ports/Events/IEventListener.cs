using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Ports.MessageBroker;
using OpenDDD.NET;

namespace OpenDDD.Infrastructure.Ports.Events
{
    public interface IEventListener<TEvent, TAction, TCommand, TReturns> : ISubscriber, IStartable, IStartableEventListener
        where TEvent : IEvent
        where TAction : IAction<TCommand, TReturns>
        where TCommand : ICommand
    {
        string ListensTo { get; set; }
        string Context { get; set; }
        BaseDomainModelVersion DomainModelVersion { get; set; }

        TCommand CreateCommand(TEvent theEvent);
        Task ReactAsync(TEvent theEvent);
    }
}
