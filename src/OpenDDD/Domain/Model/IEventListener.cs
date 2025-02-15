using Microsoft.Extensions.Hosting;

namespace OpenDDD.Domain.Model
{
    public interface IEventListener<in TEvent, TAction> : IHostedService
        where TEvent : IEvent
        where TAction : class
    {
        Task HandleAsync(TEvent @event, TAction action, CancellationToken ct);
    }
}
