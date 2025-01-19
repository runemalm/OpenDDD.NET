namespace OpenDDD.Application
{
	public interface IAction<TCommand, TReturns> where TCommand : ICommand
	{
		Task<TReturns> ExecuteAsync(TCommand command, CancellationToken ct);
	}
}
