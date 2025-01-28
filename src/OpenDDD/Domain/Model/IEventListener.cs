using Microsoft.Extensions.Hosting;

namespace OpenDDD.Domain.Model
{
	public interface IEventListener<in TEvent> : IHostedService
		where TEvent : IEvent
	{
		Task HandleAsync(TEvent @event, CancellationToken cancellationToken);
	}
}
