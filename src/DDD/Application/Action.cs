using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Domain;
using DDD.Domain.Auth;
using DDD.Infrastructure.Persistence;
using DDD.Infrastructure.Ports;

namespace DDD.Application
{
	public abstract class Action<TCommand, TReturns> : IAction<TCommand, TReturns>
		where TCommand : ICommand
	{
		protected readonly IAuthDomainService _authDomainService;
		protected readonly IDomainPublisher _domainPublisher;
		protected readonly IInterchangePublisher _interchangePublisher;
		private readonly IOutbox _outbox;
		private readonly IPersistenceService _persistenceService;

		public Action(
			IAuthDomainService authDomainService,
			IDomainPublisher domainPublisher,
			IInterchangePublisher interchangePublisher,
			IOutbox outbox,
			IPersistenceService persistenceService)
		{
			_authDomainService = authDomainService;
			_domainPublisher = domainPublisher;
			_interchangePublisher = interchangePublisher;
			_outbox = outbox;
			_persistenceService = persistenceService;
		}

		public async Task<TReturns> ExecuteAsync(
			TCommand command,
			CancellationToken ct)
		{
			var actionId = ActionId.Create();
			
			Validate(command);

			try
			{
				await _persistenceService.StartTransactionAsync(actionId);
				var result = await ExecuteAsync(command, actionId, ct);
				await SaveOutboxAsync(actionId, ct);
				await _persistenceService.CommitTransactionAsync(actionId);
				return result;
			}
			finally
			{
				// Always release connection
				await _persistenceService.ReleaseConnectionAsync(actionId);
			}
		}
		
		// Publishing
		
		public abstract Task<TReturns> ExecuteAsync(TCommand command, ActionId actionId, CancellationToken ct);

		private async Task SaveOutboxAsync(ActionId actionId, CancellationToken ct)
		{
			var domainEvents = await _domainPublisher.GetPublishedAsync(actionId);
			var integrationEvents = await _interchangePublisher.GetPublishedAsync(actionId);
			await _outbox.AddAllAsync(actionId, domainEvents, ct);
			await _outbox.AddAllAsync(actionId, integrationEvents, ct);
		}

		// Authorization
		
		public void AuthorizeRoles(
			IEnumerable<IEnumerable<string>> roles,
			CancellationToken ct)
		{
			AuthorizeRolesAsync(roles, ct).Wait();
		}

		public async Task AuthorizeRolesAsync(
			IEnumerable<IEnumerable<string>> roles,
			CancellationToken ct)
		{
			await _authDomainService.AuthorizeRolesAsync(roles, ct);
		}
		
		// Validation

		private void Validate(ICommand command)
		{
			command.Validate();
		}
	}
}
