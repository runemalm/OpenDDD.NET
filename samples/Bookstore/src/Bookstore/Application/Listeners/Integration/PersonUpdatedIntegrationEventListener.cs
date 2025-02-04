using OpenDDD.Infrastructure.Events.Base;
using OpenDDD.Infrastructure.Events;
using OpenDDD.API.Options;
using Bookstore.Application.Actions.UpdateCustomerName;
using Bookstore.Interchange.Model.Events;
using OpenDDD.API.HostedServices;

namespace Bookstore.Application.Listeners.Integration
{
    public class PersonUpdatedIntegrationEventListener : EventListenerBase<PersonUpdatedIntegrationEvent, UpdateCustomerNameAction>
    {
        public PersonUpdatedIntegrationEventListener(
            IMessagingProvider messagingProvider,
            OpenDddOptions options,
            IServiceScopeFactory serviceScopeFactory,
            StartupHostedService startupService,
            ILogger<PersonUpdatedIntegrationEventListener> logger)
            : base(messagingProvider, options, serviceScopeFactory, startupService, logger) { }

        public override async Task HandleAsync(PersonUpdatedIntegrationEvent integrationEvent, 
            UpdateCustomerNameAction action, CancellationToken ct)
        {
            var command = new UpdateCustomerNameCommand(integrationEvent.Email, integrationEvent.FullName);
            await action.ExecuteAsync(command, ct);
        }
    }
}
