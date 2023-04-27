using DDD.Infrastructure.Ports.PubSub;
using DDD.Infrastructure.Services.Persistence;

namespace DDD.Application
{
	public class TransactionalDependencies : ITransactionalDependencies
	{
		public IDomainPublisher DomainPublisher { get; }
		public IInterchangePublisher InterchangePublisher { get; }
		public IOutbox Outbox { get; }
		public IPersistenceService PersistenceService { get; }
		
		public TransactionalDependencies(
			IDomainPublisher domainPublisher,
			IInterchangePublisher interchangePublisher,
			IOutbox outbox,
			IPersistenceService persistenceService)
		{
			DomainPublisher = domainPublisher;
			InterchangePublisher = interchangePublisher;
			Outbox = outbox;
			PersistenceService = persistenceService;
		}
	}
}
