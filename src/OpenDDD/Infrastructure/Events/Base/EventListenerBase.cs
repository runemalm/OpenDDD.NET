using Microsoft.Extensions.DependencyInjection;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Helpers;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.Events.Base
{
    public abstract class EventListenerBase<TEvent, TAction> : IEventListener<TEvent, TAction>
        where TEvent : IEvent
        where TAction : class
    {
        private readonly IMessagingProvider _messagingProvider;
        private readonly OpenDddOptions _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        protected EventListenerBase(
            IMessagingProvider messagingProvider,
            OpenDddOptions options,
            IServiceScopeFactory serviceScopeFactory)
        {
            _messagingProvider = messagingProvider ?? throw new ArgumentNullException(nameof(messagingProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var topic = EventTopicHelper.DetermineTopic(typeof(TEvent), _options.EventsNamespacePrefix);
            await _messagingProvider.SubscribeAsync(topic, async (message, token) =>
            {
                var @event = EventSerializer.Deserialize<TEvent>(message);

                using var scope = _serviceScopeFactory.CreateScope();

                var action = scope.ServiceProvider.GetRequiredService<TAction>();

                await HandleAsync(@event, action, token);
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public abstract Task HandleAsync(TEvent @event, TAction action, CancellationToken cancellationToken);
    }
}
