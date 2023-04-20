using System.Threading;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Ports.PubSub
{
	public interface IDeadLetterQueue
	{
		Task EnqueueAsync(DeadEvent deadEvent, CancellationToken ct);
		void Empty(CancellationToken ct);
		Task EmptyAsync(CancellationToken ct);
	}
}
