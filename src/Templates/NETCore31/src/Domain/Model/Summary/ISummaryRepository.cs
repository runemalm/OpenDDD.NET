using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Infrastructure.Ports.Repository;

namespace Domain.Model.Summary
{
	public interface ISummaryRepository : IRepository<Summary>
	{
		Task<Summary> GetWithValueAsync(string value, ActionId actionId, CancellationToken ct);
	}
}
