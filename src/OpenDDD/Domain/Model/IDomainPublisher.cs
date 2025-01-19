namespace OpenDDD.Domain.Model
{
	public interface IDomainPublisher
	{
		Task PublishAsync(IDomainEvent domainEvent);
	}
}
