using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.TransactionalOutbox
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly OpenDddOptions _options;

        public OutboxProcessor(IServiceScopeFactory serviceScopeFactory, ILogger<OutboxProcessor> logger, OpenDddOptions options)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));
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
                    var options = scope.ServiceProvider.GetRequiredService<OpenDddOptions>();

                    var pendingEvents = await outboxRepository.GetPendingEventsAsync(stoppingToken);
            
                    foreach (var outboxEntry in pendingEvents)
                    {
                        try
                        {
                            var topic = $"{options.EventsNamespacePrefix}.{outboxEntry.EventType}.{outboxEntry.EventName}";

                            _logger.LogDebug($"Processing outbox entry: {topic}");

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

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _logger.LogInformation("Outbox Processor stopping.");
        }
    }
}
