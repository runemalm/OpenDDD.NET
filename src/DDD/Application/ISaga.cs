using System.Threading;
using System.Threading.Tasks;

namespace DDD.Application
{
	public interface ISaga<C, R>
	{
		Task<R> ExecuteAsync(C command, CancellationToken ct);
	}
}
