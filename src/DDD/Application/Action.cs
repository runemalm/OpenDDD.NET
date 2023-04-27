using System;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application.Error;

namespace DDD.Application
{
	public abstract class Action<TCommand, TReturns> : IAction<TCommand, TReturns>
		where TCommand : ICommand
	{
		private readonly ITransactionalDependencies _trxDeps;

		public Action(ITransactionalDependencies transactionalDependencies)
		{
			_trxDeps = transactionalDependencies;
		}

		public async Task<TReturns> ExecuteAsync(
			TCommand command,
			CancellationToken ct)
		{
			var actionId = ActionId.Create();
			
			Validate(command);

			try
			{
				await _trxDeps.PersistenceService.StartTransactionAsync(actionId);
				var result = await ExecuteAsync(command, actionId, ct);
				await SaveOutboxAsync(actionId, ct);
				await _trxDeps.PersistenceService.CommitTransactionAsync(actionId);
				return result;
			}
			catch (Exception e)
			{
				if (_trxDeps == null)
					throw TransactionalException.NotRegistered(e);
				throw;
			}
			finally
			{
				// Always release connection
				await _trxDeps.PersistenceService.ReleaseConnectionAsync(actionId);
			}
		}
		
		// Publishing
		
		public abstract Task<TReturns> ExecuteAsync(TCommand command, ActionId actionId, CancellationToken ct);

		private async Task SaveOutboxAsync(ActionId actionId, CancellationToken ct)
		{
			var domainEvents = await _trxDeps.DomainPublisher.GetPublishedAsync(actionId);
			var integrationEvents = await _trxDeps.InterchangePublisher.GetPublishedAsync(actionId);
			await _trxDeps.Outbox.AddAllAsync(actionId, domainEvents, ct);
			await _trxDeps.Outbox.AddAllAsync(actionId, integrationEvents, ct);
		}

		// Validation

		private void Validate(ICommand command)
		{
			command.Validate();
		}
	}
}
