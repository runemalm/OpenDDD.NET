using System.Threading;
using System.Threading.Tasks;

namespace OpenDDD.Application
{
	public interface IAction<TCommand, TReturns>
		where TCommand : ICommand
	{
		Task<TReturns> ExecuteAsync(TCommand command, CancellationToken ct);
		Task<TReturns> ExecuteAsync(TCommand command, ActionId actionId, CancellationToken ct);
	}
}
