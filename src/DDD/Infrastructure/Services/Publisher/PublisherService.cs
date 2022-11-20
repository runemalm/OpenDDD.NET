using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Domain;
using DDD.Domain.Model;
using DDD.Domain.Model.BuildingBlocks;
using DDD.Domain.Model.BuildingBlocks.Event;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Infrastructure.Services.Persistence;
using DDD.Logging;

namespace DDD.Infrastructure.Services.Publisher
{
    public class PublisherService : IPublisherService
    {
        private readonly ILogger _logger;
        protected readonly IDomainPublisher _domainPublisher;
        protected readonly IInterchangePublisher _interchangePublisher;
        private readonly IOutbox _outbox;
        private readonly IPersistenceService _persistenceService;
        private bool _isStarted;
        public CancellationTokenSource _waitCancellationTokenSource;

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
            _isStarted = true;

            while (!stoppingToken.IsCancellationRequested && _isStarted)
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
                    await Task.Delay(1000, _waitCancellationTokenSource.Token);
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

        private async Task MarkAsNotPublishingAsync(OutboxEvent outboxEvent)
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
