using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Infrastructure.Ports.PubSub;

namespace DDD.Infrastructure.Ports.Adapters.PubSub.Memory
{
	public class MemoryDeadLetterQueue : IDeadLetterQueue
	{
		private ICollection<DeadEvent> _deadEvents;
		
		public MemoryDeadLetterQueue()
		{
			_deadEvents = new List<DeadEvent>();
		}

		public Task EnqueueAsync(DeadEvent deadEvent, CancellationToken ct)
		{
			_deadEvents.Add(deadEvent);
			return Task.CompletedTask;
		}
		
		public void Empty(CancellationToken ct)
		{
			_deadEvents = new List<DeadEvent>();
		}

		public Task EmptyAsync(CancellationToken ct)
		{
			Empty(ct);
			return Task.CompletedTask;
		}
	}
}
