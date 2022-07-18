using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DDD.Domain;

namespace DDD.Infrastructure.Ports.Adapters.Repositories.Memory
{
	public class MemoryEventRepository : IEventRepository
	{
		private IDictionary<ActionId, ICollection<IEvent>> _events;
		
		public MemoryEventRepository()
		{
			_events = new Dictionary<ActionId, ICollection<IEvent>>();
		}

		public Task SaveAllAsync(ActionId actionId, IEnumerable<IEvent> events, CancellationToken ct)
		{
			if (!_events.ContainsKey(actionId))
				_events.Add(actionId, new List<IEvent>());
			(_events[actionId].ToList()).AddRange(events);
			return Task.CompletedTask;
		}

		public Task<IEnumerable<OutboxEvent>> GetAllAsync(ActionId actionId, CancellationToken ct)
		{
			var events = new List<IEvent>();
			
			if (_events.ContainsKey(actionId))
				events = _events[actionId].ToList();
			
			var outboxEvents = new List<OutboxEvent>();

			foreach (var e in events)
			{
				outboxEvents.Add(new OutboxEvent(e));
			}

			return Task.FromResult(outboxEvents.AsEnumerable());
		}

		public Task DeleteAsync(EventId eventId, ActionId actionId, CancellationToken ct)
		{
			if (_events.ContainsKey(actionId))
				_events[actionId] = _events[actionId].Where(e => e.EventId != eventId).ToList();
			return Task.CompletedTask;
		}

		public Task DeleteAllAsync(CancellationToken ct)
		{
			_events = new Dictionary<ActionId, ICollection<IEvent>>();
			return Task.CompletedTask;
		}
	}
}
