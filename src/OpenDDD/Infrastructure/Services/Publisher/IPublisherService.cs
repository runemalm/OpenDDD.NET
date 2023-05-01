using System.Threading;
using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Services.Publisher
{
	public interface IPublisherService : IInfrastructureService
	{
		void Start();
		void Stop();
		Task WorkOutboxAsync(CancellationToken stoppingToken);
		Task StopWorkingOutboxAsync(CancellationToken stoppingToken);
	}
}
