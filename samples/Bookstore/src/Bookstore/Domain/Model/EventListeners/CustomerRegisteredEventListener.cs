using Bookstore.Domain.Model.Events;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Helpers;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Main.Options;

namespace Bookstore.Domain.Model.EventListeners
{
    public class CustomerRegisteredEventListener : IDomainEventListener
    {
        private readonly IMessagingProvider _messagingProvider;
        private readonly string _namespacePrefix;

        public CustomerRegisteredEventListener(IMessagingProvider messagingProvider, OpenDddOptions options)
        {
            _messagingProvider = messagingProvider;
            _namespacePrefix = options.EventsNamespacePrefix;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var topic = EventTopicHelper.DetermineTopic(typeof(CustomerRegisteredDomainEvent), _namespacePrefix);
            await _messagingProvider.SubscribeAsync(topic, HandleMessage);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private Task HandleMessage(string message)
        {
            Console.WriteLine($"CustomerRegisteredEventListener received message: {message}");
            return Task.CompletedTask;
        }
    }
}
