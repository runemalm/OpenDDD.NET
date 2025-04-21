using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenDDD.API.HostedServices;
using OpenDDD.API.Options;
using OpenDDD.Domain.Model.Helpers;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using OpenDDD.Infrastructure.TransactionalOutbox.Options;

namespace OpenDDD.Infrastructure.TransactionalOutbox
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly StartupHostedService _startupService;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly OpenDddOptions _options;
        private readonly OpenDddOutboxProcessorOptions _processorOptions;

        public OutboxProcessor(
            IServiceScopeFactory serviceScopeFactory,
            StartupHostedService startupService,
            ILogger<OutboxProcessor> logger,
            IOptions<OpenDddOptions> options,
            IOptions<OpenDddOutboxProcessorOptions> processorOptions)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _startupService = startupService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _processorOptions = processorOptions.Value ?? throw new ArgumentNullException(nameof(processorOptions));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor started.");
            _logger.LogDebug("Polling interval: {PollingIntervalSeconds}s, Max events per cycle: {MaxEventsPerCycle}",
                _processorOptions.PollingIntervalSeconds,
                _processorOptions.MaxEventsPerCycle);

            _logger.LogInformation("Waiting for database setup to complete before starting outbox processing...");
            await _startupService.StartupCompleted;
            _logger.LogInformation("Database setup completed. Starting outbox processing...");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceScopeFactory.CreateScope();

                try
                {
                    var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                    var messagingProvider = scope.ServiceProvider.GetRequiredService<IMessagingProvider>();
                    var databaseSession = scope.ServiceProvider.GetService<IDatabaseSession>();

                    if (databaseSession == null)
                    {
                        _logger.LogError("No valid database session found for persistence provider: {PersistenceProvider}", _options.PersistenceProvider);
                        return;
                    }

                    await databaseSession.OpenConnectionAsync(stoppingToken);

                    var pendingEvents = await outboxRepository
                        .GetPendingEventsAsync(_processorOptions.MaxEventsPerCycle, stoppingToken);

                    _logger.Log(
                        pendingEvents.Any() ? LogLevel.Debug : LogLevel.Trace,
                        "Fetched {EventCount} pending events.", pendingEvents.Count);

                    foreach (var outboxEntry in pendingEvents)
                    {
                        try
                        {
                            var topic = EventTopicHelper.DetermineTopic(
                                outboxEntry.EventType,
                                outboxEntry.EventName,
                                _options.Events,
                                _logger);

                            _logger.LogDebug("Publishing outbox event {EventId} to topic {Topic}", outboxEntry.Id, topic);

                            await messagingProvider.PublishAsync(topic, outboxEntry.Payload, stoppingToken);
                            await outboxRepository.MarkEventAsProcessedAsync(outboxEntry.Id, stoppingToken);
                            
                            _logger.LogDebug("Successfully published and marked event {EventId} as processed.", outboxEntry.Id);
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

                await Task.Delay(TimeSpan.FromSeconds(_processorOptions.PollingIntervalSeconds), stoppingToken);
            }

            _logger.LogInformation("Outbox Processor stopping.");
        }
    }
}
