using System.Threading;
using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Services.Publisher
{
	public interface IPublisherService : IInfrastructureService
	{
		Task WorkOutboxAsync(CancellationToken stoppingToken);
		Task StopWorkingOutboxAsync(CancellationToken stoppingToken);
	}
}
