namespace OpenDDD.Domain.Model
{
    public interface IIntegrationPublisher
    {
        Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    }
}
