using DDD.Infrastructure.Ports.PubSub;
using DDD.Infrastructure.Services.Persistence;

namespace DDD.Application
{
	public interface ITransactionalDependencies
	{
		IDomainPublisher DomainPublisher { get; }
		IInterchangePublisher InterchangePublisher { get; }
		IOutbox Outbox { get; }
		IPersistenceService PersistenceService { get; }
	}
}
