using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDD.NET.Services.DatabaseConnection;
using OpenDDD.NET.Services.MessageBrokerConnection;

namespace OpenDDD.Infrastructure.Services.EventProcessor
{
    public class EventProcessorHostedService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private CancellationTokenSource? _cts;
        protected readonly IEventProcessor _eventProcessor;
        protected readonly IEventProcessorDatabaseConnection _eventProcessorOutboxDatabaseConnection;
        protected readonly IDomainMessageBrokerConnection _domainMessageBrokerConnection;
        protected readonly IEventProcessorSettings _eventProcessorSettings;

        public EventProcessorHostedService(ILogger<EventProcessorHostedService> logger, IEventProcessor eventProcessor, IEventProcessorDatabaseConnection eventProcessorOutboxDatabaseConnection, IDomainMessageBrokerConnection domainMessageBrokerConnection, IEventProcessorSettings eventProcessorSettings)
        {
            _logger = logger;
            _eventProcessor = eventProcessor;
            _eventProcessorOutboxDatabaseConnection = eventProcessorOutboxDatabaseConnection;
            _domainMessageBrokerConnection = domainMessageBrokerConnection;
            _eventProcessorSettings = eventProcessorSettings;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_eventProcessorSettings.Enabled)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                // Start the background task
                Task.Run(() => RunInBackground(_cts.Token));

                _logger.LogInformation("Background service started.");
            }

            return Task.CompletedTask;
        }

        private async Task RunInBackground(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Background task is running...");
                
                var didProcess = await _eventProcessor.ProcessNextOutboxEventAsync();
                
                if (!didProcess)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
            }

            _logger.LogInformation("Background task is stopping...");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Request cancellation and wait for the background task to complete
            _cts!.Cancel();

            _logger.LogInformation("Background service stopped.");
            
            // Disconnect
            await _eventProcessorOutboxDatabaseConnection.StopAsync(cancellationToken);
            await _domainMessageBrokerConnection.StopAsync(cancellationToken);
        }

        public void Dispose()
        {
            _cts?.Dispose();
        }
        
        
        
        
        //
        //
        // // Loop
        //
        // protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        // {
        //     try
        //     {
        //         _logger.LogError($"TODO: Start the event processor infrastructure service.");
        //         // await _eventProcessor.StartAsync(stoppingToken);
        //     }
        //     catch (Exception e)
        //     {
        //         _logger.LogError($"Event processor threw exception when being started from hosted service: {e}", e);
        //     }
        // }
        //
        // public override async Task StopAsync(CancellationToken cancellationToken)
        // {
        //     try
        //     {
        //         _logger.LogError($"TODO: Stop the event processor infrastructure service.");
        //         // await _eventProcessor.StopAsync(cancellationToken);
        //     }
        //     catch (Exception e)
        //     {
        //         _logger.LogError($"Event processor threw exception when being stopped from hosted service: {e}", e);
        //     }
        //     await base.StopAsync(cancellationToken);
        // }
    }
}
