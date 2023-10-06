using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Services.EventProcessor
{
	public interface IEventProcessor : IInfrastructureService
	{
		Task ProcessNextOutboxEventAsync();
	}
}
