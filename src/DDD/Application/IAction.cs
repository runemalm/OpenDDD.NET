using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Domain;

namespace DDD.Application
{
	public interface IAction<TCommand, TReturns>
		where TCommand : ICommand
	{
		void AuthorizeRoles(IEnumerable<IEnumerable<string>> roles, CancellationToken ct);
		Task AuthorizeRolesAsync(IEnumerable<IEnumerable<string>> roles, CancellationToken ct);
		Task<TReturns> ExecuteAsync(TCommand command, CancellationToken ct);
	}
}
