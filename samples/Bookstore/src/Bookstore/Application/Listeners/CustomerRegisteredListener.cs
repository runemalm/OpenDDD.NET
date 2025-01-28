using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Helpers;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Main.Options;
using Bookstore.Application.Actions.SendWelcomeEmail;
using Bookstore.Domain.Model.Events;

namespace Bookstore.Application.Listeners
{
    public class CustomerRegisteredListener : IDomainEventListener<CustomerRegistered>
    {
        private readonly SendWelcomeEmailAction _sendWelcomeEmailAction;
        private readonly IMessagingProvider _messagingProvider;
        private readonly string _namespacePrefix;

        public CustomerRegisteredListener(
            SendWelcomeEmailAction sendWelcomeEmailAction, 
            IMessagingProvider messagingProvider,
            OpenDddOptions options)
        {
            _sendWelcomeEmailAction = sendWelcomeEmailAction ?? throw new ArgumentNullException(nameof(sendWelcomeEmailAction));
            _messagingProvider = messagingProvider;
            _namespacePrefix = options.EventsNamespacePrefix;
        }
        
        public async Task HandleAsync(CustomerRegistered domainEvent, CancellationToken ct)
        {
            if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));

            var command = new SendWelcomeEmailCommand(domainEvent.Email, domainEvent.Name);
            await _sendWelcomeEmailAction.ExecuteAsync(command, ct);
        }

        public async Task StartAsync(CancellationToken ct)
        {
            var topic = EventTopicHelper.DetermineTopic(typeof(CustomerRegistered), _namespacePrefix);
            await _messagingProvider.SubscribeAsync(topic, async (message, token) =>
            {
                var domainEvent = EventSerializer.Deserialize<CustomerRegistered>(message);
                await HandleAsync(domainEvent, token);
            }, ct);
        }

        public Task StopAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }
    }
}
