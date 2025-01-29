using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model.Helpers;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.TransactionalOutbox
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly OpenDddEventsOptions _eventOptions;

        public OutboxProcessor(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxProcessor> logger, OpenDddOptions options)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventOptions = options?.Events ?? throw new ArgumentNullException(nameof(options.Events));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                    var messagingProvider = scope.ServiceProvider.GetRequiredService<IMessagingProvider>();

                    var pendingEvents = await outboxRepository.GetPendingEventsAsync(stoppingToken);

                    foreach (var outboxEntry in pendingEvents)
                    {
                        try
                        {
                            // Determine topic dynamically using helper
                            var topic = EventTopicHelper.DetermineTopic(outboxEntry.EventType, 
                                outboxEntry.EventName, _eventOptions, _logger);

                            _logger.LogDebug("Publishing outbox event {EventId} to topic {Topic}", outboxEntry.Id, topic);

                            await messagingProvider.PublishAsync(topic, outboxEntry.Payload, stoppingToken);
                            await outboxRepository.MarkEventAsProcessedAsync(outboxEntry.Id, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to process outbox event {EventId}.", outboxEntry.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in Outbox Processor.");
                }

                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }

            _logger.LogInformation("Outbox Processor stopping.");
        }
    }
}
