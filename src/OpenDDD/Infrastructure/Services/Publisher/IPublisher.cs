using System.Threading.Tasks;
using OpenDDD.Domain.Model.Event;

namespace OpenDDD.Infrastructure.Services.Publisher
{
	public interface IPublisher<T> 
		where T : IEvent
	{
		Task PublishAsync(T theEvent);
	}
}
