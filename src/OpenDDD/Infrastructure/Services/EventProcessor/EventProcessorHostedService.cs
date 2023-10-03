using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenDDD.Infrastructure.Services.EventProcessor
{
    public class EventProcessorHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        protected readonly IEventProcessor _eventProcessor;

        public EventProcessorHostedService(ILogger<EventProcessorHostedService> logger, IEventProcessor eventProcessor)
        {
            _logger = logger;
            _eventProcessor = eventProcessor;
        }
        
        // Loop

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogError($"TODO: Start the event processor infrastructure service.");
                // await _eventProcessor.StartAsync(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError($"Event processor threw exception when being started from hosted service: {e}", e);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogError($"TODO: Stop the event processor infrastructure service.");
                // await _eventProcessor.StopAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError($"Event processor threw exception when being stopped from hosted service: {e}", e);
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
