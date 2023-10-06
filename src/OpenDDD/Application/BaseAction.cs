using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenDDD.NET.Services.DatabaseConnection;

namespace OpenDDD.Application
{
	public abstract class BaseAction<TCommand, TReturns> : IAction<TCommand, TReturns>
		where TCommand : ICommand
	{
		private readonly IActionDatabaseConnection _actionDatabaseConnection;
		private readonly ILogger _logger;

		public BaseAction(IActionDatabaseConnection actionDatabaseConnection, ILogger<BaseAction<TCommand, TReturns>> logger)
		{
			_actionDatabaseConnection = actionDatabaseConnection;
			_logger = logger;
		}
		
		public abstract Task<TReturns> ExecuteAsync(TCommand command, ActionId actionId, CancellationToken ct);

		public async Task<TReturns> ExecuteTrxAsync(
			TCommand command,
			ActionId actionId,
			CancellationToken ct)
		{
			_logger.LogDebug("Executing action.");
			try
			{
				await _actionDatabaseConnection.StartAsync(CancellationToken.None);
				await _actionDatabaseConnection.StartTransactionAsync();
				var result = await ExecuteAsync(command, actionId, ct);
				await _actionDatabaseConnection.CommitTransactionAsync();
				_logger.LogDebug("Action executed.");
				return result;
			}
			catch (Exception)
			{
				await _actionDatabaseConnection.RollbackTransactionAsync();
				_logger.LogError("Action failed!");
				throw;
			}
			finally
			{
				await _actionDatabaseConnection.StopAsync(CancellationToken.None);
			}
		}
	}
}
