using OpenDDD.Infrastructure.Events.Base;
using OpenDDD.Main.Options;
using OpenDDD.Infrastructure.Events;
using Bookstore.Application.Actions.UpdateCustomerName;
using Bookstore.Domain.Model.Events.Integration;

namespace Bookstore.Application.Listeners.Integration
{
    public class PersonUpdatedIntegrationListener : EventListenerBase<PersonUpdatedIntegrationEvent, UpdateCustomerNameAction>
    {
        public PersonUpdatedIntegrationListener(
            IMessagingProvider messagingProvider,
            OpenDddOptions options,
            IServiceScopeFactory serviceScopeFactory)
            : base(messagingProvider, options, serviceScopeFactory) { }

        public override async Task HandleAsync(PersonUpdatedIntegrationEvent integrationEvent, 
            UpdateCustomerNameAction action, CancellationToken ct)
        {
            var command = new UpdateCustomerNameCommand(integrationEvent.Email, integrationEvent.FullName);
            await action.ExecuteAsync(command, ct);
        }
    }
}
