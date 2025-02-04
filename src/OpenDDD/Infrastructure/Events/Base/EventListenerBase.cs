using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenDDD.API.HostedServices;
using OpenDDD.API.Options;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Helpers;

namespace OpenDDD.Infrastructure.Events.Base
{
    public abstract class EventListenerBase<TEvent, TAction> : IEventListener<TEvent, TAction>
        where TEvent : IEvent
        where TAction : class
    {
        private readonly IMessagingProvider _messagingProvider;
        private readonly OpenDddOptions _options;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly StartupHostedService _startupService;
        private readonly ILogger<EventListenerBase<TEvent, TAction>> _logger;

        protected EventListenerBase(
            IMessagingProvider messagingProvider,
            OpenDddOptions options,
            IServiceScopeFactory serviceScopeFactory,
            StartupHostedService startupService,
            ILogger<EventListenerBase<TEvent, TAction>> logger)
        {
            _messagingProvider = messagingProvider ?? throw new ArgumentNullException(nameof(messagingProvider));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken ct)
        {
            var topic = EventTopicHelper.DetermineTopic(typeof(TEvent), _options.Events, _logger);
            var consumerGroup = _options.Events.ListenerGroup;
            
            _logger.LogInformation("Waiting for startup service to finish before subscribing to events...");
            await _startupService.StartupCompleted;
            _logger.LogInformation("Startup service finished. Subscribing to events...");

            _logger.LogInformation("Subscribing to topic '{Topic}' in consumer group '{ConsumerGroup}'.", topic, consumerGroup);

            await _messagingProvider.SubscribeAsync(topic, consumerGroup, async (message, token) =>
            {
                try
                {
                    var @event = EventSerializer.Deserialize<TEvent>(message);
                    _logger.LogInformation("Received event '{EventType}' from topic '{Topic}'.", typeof(TEvent).Name, topic);

                    using var scope = _serviceScopeFactory.CreateScope();
                    var action = scope.ServiceProvider.GetRequiredService<TAction>();

                    _logger.LogInformation("Executing action '{ActionType}' for event '{EventType}'.", typeof(TAction).Name, typeof(TEvent).Name);
                    
                    await HandleAsync(@event, action, token);
                    
                    _logger.LogInformation("Successfully processed event '{EventType}'.", typeof(TEvent).Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event '{EventType}'.", typeof(TEvent).Name);
                }
            }, ct);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public abstract Task HandleAsync(TEvent @event, TAction action, CancellationToken cancellationToken);
    }
}
