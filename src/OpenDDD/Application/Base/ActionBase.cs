namespace OpenDDD.Application.Base
{
	public abstract class ActionBase<TCommand, TReturns> : IAction<TCommand, TReturns>
		where TCommand : ICommand
	{
		public abstract Task<TReturns> ExecuteAsync(TCommand command, CancellationToken ct);
	}
}
