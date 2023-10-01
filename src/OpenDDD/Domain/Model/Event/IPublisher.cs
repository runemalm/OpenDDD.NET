using System.Threading.Tasks;

namespace OpenDDD.Domain.Model.Event
{
	public interface IPublisher<T> 
		where T : IEvent
	{
		Task PublishAsync(T theEvent);
	}
}
