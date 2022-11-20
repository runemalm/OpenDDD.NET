using System;
using System.Threading;
using System.Threading.Tasks;
using DDD.Infrastructure.Services.Publisher;
using Microsoft.Extensions.Hosting;
using DDD.Logging;

namespace DDD.NETCore.HostedServices
{
    public class PublisherHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        protected readonly IPublisherService _publisherService;

        public PublisherHostedService(ILogger logger, IPublisherService publisherService)
        {
            _logger = logger;
            _publisherService = publisherService;
        }
        
        // Loop

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _publisherService.WorkOutboxAsync(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.Log($"Publisher service threw exception in hosted service: {e}", LogLevel.Error);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _publisherService.StopWorkingOutboxAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.Log($"Publisher service threw exception in hosted service: {e}", LogLevel.Error);
            }
            await base.StopAsync(cancellationToken);
        }
    }
}
