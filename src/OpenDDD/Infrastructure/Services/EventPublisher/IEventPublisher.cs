using System.Threading.Tasks;
using OpenDDD.Domain.Model.Event;

namespace OpenDDD.Infrastructure.Services.EventPublisher
{
	public interface IEventPublisher<T> : IMessagePublisher 
		where T : IEvent
	{
		Task PublishAsync(T theEvent);
	}
}
