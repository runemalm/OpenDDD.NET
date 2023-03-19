using System.Threading;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Ports.Repository
{
	public interface IStartableRepository
	{
		Task StartAsync(CancellationToken ct);
		Task StopAsync(CancellationToken ct);
	}
}
