using System.Threading;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Ports
{
	public interface IDeadLetterQueue
	{
		Task EnqueueAsync(DeadEvent deadEvent, CancellationToken ct);
		Task EmptyAsync(CancellationToken ct);
	}
}
