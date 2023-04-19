using System.Threading;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Ports.Repository
{
	public interface IStartableRepository
	{
		void Start(CancellationToken ct);
		Task StartAsync(CancellationToken ct);
		void Stop(CancellationToken ct);
		Task StopAsync(CancellationToken ct);
	}
}
