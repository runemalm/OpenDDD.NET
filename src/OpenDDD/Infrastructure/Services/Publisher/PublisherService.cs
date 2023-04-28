using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Domain.Model.BuildingBlocks.Event;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.Infrastructure.Services.Persistence;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Services.Publisher
{
    public class PublisherService : IPublisherService
    {
        private readonly ILogger _logger;
        private readonly IDomainPublisher _domainPublisher;
        private readonly IInterchangePublisher _interchangePublisher;
        private readonly IOutbox _outbox;
        private readonly IPersistenceService _persistenceService;
        private bool _isStarted;
        private readonly CancellationTokenSource _waitCancellationTokenSource;

        public PublisherService(
            ILogger logger, 
            IDomainPublisher domainPublisher,
            IInterchangePublisher interchangePublisher,
            IOutbox outbox,
            IPersistenceService persistenceService)
        {
            _logger = logger;
            _domainPublisher = domainPublisher;
            _interchangePublisher = interchangePublisher;
            _outbox = outbox;
            _persistenceService = persistenceService;
            _isStarted = false;
            _waitCancellationTokenSource = new CancellationTokenSource();
        }

        public async Task WorkOutboxAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested && 
                   !_waitCancellationTokenSource.Token.IsCancellationRequested &&
                   !_isStarted)
            {
                try
                {
                    await Task.Delay(500, _waitCancellationTokenSource.Token);
                }
                catch (TaskCanceledException e)
                {
                    _logger.Log("Wait cancellationtoken was canceled while publisherservice was waiting to be started.", LogLevel.Debug);
                }
            }

            while (!stoppingToken.IsCancellationRequested && 
                   !_waitCancellationTokenSource.Token.IsCancellationRequested &&
                   _isStarted)
            {
                var outboxEvent = await GetNextAsync();

                if (outboxEvent != null)
                {
                    var didPublish = true;

                    try
                    {
                        await PublishAsync(outboxEvent);
                    }
                    catch (Exception e)
                    {
                        _logger.Log($"Exception when publishing in publisher: {e}", LogLevel.Error);
                        didPublish = false;

                    }

                    if (didPublish)
                        await DeleteFromOutboxAsync(outboxEvent);
                    else
                        await MarkAsNotPublishingAsync(outboxEvent);
                }

                if (outboxEvent == null)
                {
                    try
                    {
                        await Task.Delay(1000, _waitCancellationTokenSource.Token);
                    }
                    catch (TaskCanceledException e)
                    {
                        _logger.Log("Wait cancellationtoken was canceled while publisherservice was waiting for next work cycle in started mode.", LogLevel.Debug);
                    }
                }
            }
        }

        public Task StopWorkingOutboxAsync(CancellationToken stoppingToken)
        {
            _isStarted = false;
            _waitCancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
        
        // Helpers

        private Task<OutboxEvent> GetNextAsync()
            => _outbox.GetNextAsync(CancellationToken.None);

        private Task MarkAsNotPublishingAsync(OutboxEvent outboxEvent)
            => _outbox.MarkAsNotPublishingAsync(outboxEvent.Id, CancellationToken.None);

        private async Task PublishAsync(OutboxEvent outboxEvent)
        {
            if (outboxEvent.IsDomainEvent)
                await _domainPublisher.FlushAsync(outboxEvent);
            else
                await _interchangePublisher.FlushAsync(outboxEvent);
        }
        
        private async Task DeleteFromOutboxAsync(OutboxEvent outboxEvent)
        {
            await _outbox.RemoveAsync(outboxEvent.Id, CancellationToken.None);
        }
        
        private async Task AddToOutboxAsync(IEvent theEvent)
        {
            await _outbox.AddAllAsync(
                theEvent.Header.ActionId, 
                new List<IEvent> { theEvent }, 
                CancellationToken.None);
        }
    }
}
