namespace OpenDDD.Domain.Model
{
    public interface IDomainPublisher
    {
        Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default) 
            where TDomainEvent : IDomainEvent;
    }
}
