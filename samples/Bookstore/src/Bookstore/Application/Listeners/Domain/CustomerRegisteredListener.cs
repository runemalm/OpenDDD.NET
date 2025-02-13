using OpenDDD.Infrastructure.Events.Base;
using OpenDDD.Infrastructure.Events;
using OpenDDD.API.Options;
using OpenDDD.API.HostedServices;
using Bookstore.Application.Actions.SendWelcomeEmail;
using Bookstore.Domain.Model.Events;

namespace Bookstore.Application.Listeners.Domain
{
    public class CustomerRegisteredListener : EventListenerBase<CustomerRegistered, SendWelcomeEmailAction>
    {
        public CustomerRegisteredListener(
            IMessagingProvider messagingProvider,
            OpenDddOptions options,
            IServiceScopeFactory serviceScopeFactory,
            StartupHostedService startupService,
            ILogger<CustomerRegisteredListener> logger)
            : base(messagingProvider, options, serviceScopeFactory, startupService, logger) { }

        public override async Task HandleAsync(CustomerRegistered domainEvent, SendWelcomeEmailAction action, CancellationToken ct)
        {
            var command = new SendWelcomeEmailCommand(domainEvent.Email, domainEvent.Name);
            await action.ExecuteAsync(command, ct);
        }
    }
}
