using System.Collections.Generic;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Domain.Model.BuildingBlocks.Event;

namespace OpenDDD.Infrastructure.Ports.PubSub
{
	public interface IPublisher
	{
		Task PublishAsync(IEvent theEvent);
		Task FlushAsync(OutboxEvent outboxEvent);
		void SetEnabled(bool enabled);
		bool HasPublished(IEvent theEvent);
		bool HasFlushed(IEvent outboxEvent);
		Task<IEnumerable<IEvent>> GetPublishedAsync(ActionId actionId);
	}
}
