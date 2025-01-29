using OpenDDD.Infrastructure.Events.Base;
using OpenDDD.Main.Options;
using OpenDDD.Infrastructure.Events;
using Bookstore.Application.Actions.SendWelcomeEmail;
using Bookstore.Domain.Model.Events.Domain;

namespace Bookstore.Application.Listeners.Domain
{
    public class CustomerRegisteredListener : EventListenerBase<CustomerRegistered, SendWelcomeEmailAction>
    {
        public CustomerRegisteredListener(
            IMessagingProvider messagingProvider,
            OpenDddOptions options,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CustomerRegisteredListener> logger)
            : base(messagingProvider, options, serviceScopeFactory, logger) { }

        public override async Task HandleAsync(CustomerRegistered domainEvent, SendWelcomeEmailAction action, CancellationToken ct)
        {
            var command = new SendWelcomeEmailCommand(domainEvent.Email, domainEvent.Name);
            await action.ExecuteAsync(command, ct);
        }
    }
}
