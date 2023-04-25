using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Infrastructure.Ports.Adapters.Repository.Memory;
using Domain.Model.Summary;

namespace Infrastructure.Ports.Adapters.Repositories.Memory
{
	public class MemorySummaryRepository : MemoryRepository<Summary>, ISummaryRepository
	{
		public Task<Summary> GetWithValueAsync(string value, ActionId actionId, CancellationToken ct)
			=> GetFirstOrDefaultWithAsync(s => s.Value == value, actionId, ct);
	}
}
