using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

		public Task EmptyAsync(CancellationToken ct)
		{
			_deadEvents = new List<DeadEvent>();
			return Task.CompletedTask;
		}
	}
}
