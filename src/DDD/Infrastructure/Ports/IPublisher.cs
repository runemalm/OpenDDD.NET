using System.Collections.Generic;
using System.Threading.Tasks;
using DDD.Domain;

namespace DDD.Infrastructure.Ports
{
	public interface IPublisher
	{
		Task PublishAsync(IEvent theEvent);
		Task FlushAsync(OutboxEvent outboxEvent);
		bool HasPublished(IEvent theEvent);
		bool HasFlushed(IEvent outboxEvent);
		Task<IEnumerable<IEvent>> GetPublishedAsync(ActionId actionId);
	}
}
