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
		private readonly IEventRepository _eventRepository;
		private readonly IPersistenceService _persistenceService;

		public Action(
			IAuthDomainService authDomainService,
			IDomainPublisher domainPublisher,
			IInterchangePublisher interchangePublisher,
			IEventRepository eventRepository,
			IPersistenceService persistenceService)
		{
			_authDomainService = authDomainService;
			_domainPublisher = domainPublisher;
			_interchangePublisher = interchangePublisher;
			_eventRepository = eventRepository;
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
				await FlushOutboxAsync(actionId, ct);
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
			await _eventRepository.SaveAllAsync(actionId, domainEvents, ct);
			await _eventRepository.SaveAllAsync(actionId, integrationEvents, ct);
		}

		private async Task FlushOutboxAsync(ActionId actionId, CancellationToken ct)
		{
			var outboxEvents = await _eventRepository.GetAllAsync(actionId, ct);
			foreach (var outboxEvent in outboxEvents)
			{
				if (outboxEvent.IsDomainEvent)
					await _domainPublisher.FlushAsync(outboxEvent);
				else
					await _interchangePublisher.FlushAsync(outboxEvent);
				await _eventRepository.DeleteAsync(outboxEvent.EventId, actionId, ct);
			}
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
