using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Infrastructure.Ports.Repository;

namespace Domain.Model.Summary
{
	public interface ISummaryRepository : IRepository<Summary>
	{
		Task<Summary> GetWithValueAsync(string value, ActionId actionId, CancellationToken ct);
	}
}
