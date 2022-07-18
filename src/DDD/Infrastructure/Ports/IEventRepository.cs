using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Domain;

namespace DDD.Infrastructure.Ports
{
	public interface IEventRepository
	{
		Task SaveAllAsync(ActionId actionId, IEnumerable<IEvent> events, CancellationToken ct);
		Task<IEnumerable<OutboxEvent>> GetAllAsync(ActionId actionId, CancellationToken ct);
		Task DeleteAsync(EventId eventId, ActionId actionId, CancellationToken ct);
		Task DeleteAllAsync(CancellationToken ct);
	}
}
