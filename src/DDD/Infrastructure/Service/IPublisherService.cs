using System.Threading;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Service
{
	public interface IPublisherService : IInfrastructureService
	{
		Task WorkOutboxAsync(CancellationToken stoppingToken);
		Task StopWorkingOutboxAsync(CancellationToken stoppingToken);
	}
}
