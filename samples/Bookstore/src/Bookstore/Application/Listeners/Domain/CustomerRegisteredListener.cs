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
            IServiceScopeFactory serviceScopeFactory)
            : base(messagingProvider, options, serviceScopeFactory) { }

        public override async Task HandleAsync(CustomerRegistered domainEvent, SendWelcomeEmailAction action, CancellationToken ct)
        {
            var command = new SendWelcomeEmailCommand(domainEvent.Email, domainEvent.Name);
            await action.ExecuteAsync(command, ct);
        }
    }
}
