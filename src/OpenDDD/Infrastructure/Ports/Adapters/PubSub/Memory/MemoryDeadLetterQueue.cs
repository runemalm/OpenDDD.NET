using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Infrastructure.Ports.PubSub;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.Memory
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
