using DDD.Infrastructure.Ports.Adapters.Repository.Memory;
using Domain.Model.Forecast;

namespace Infrastructure.Ports.Adapters.Repositories.Memory
{
	public class MemoryForecastRepository : MemoryRepository<Forecast>, IForecastRepository
	{
		
	}
}
