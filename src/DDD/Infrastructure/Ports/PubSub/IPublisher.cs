using System.Collections.Generic;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain;
using DDD.Domain.Model;
using DDD.Domain.Model.BuildingBlocks;
using DDD.Domain.Model.BuildingBlocks.Event;

namespace DDD.Infrastructure.Ports.PubSub
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
