using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Services.EventPublisher;
using OpenDDD.NET.Services.Outbox;

namespace OpenDDD.Infrastructure.Services.EventProcessor
{
    public class EventProcessor : IEventProcessor
    {
        private readonly ILogger _logger;
        private readonly IDomainEventPublisher _domainEventPublisher;
        private readonly IEventProcessorOutbox _outbox;
        private readonly CancellationTokenSource _waitCancellationTokenSource;

        public EventProcessor(
            ILogger<EventProcessor> logger,
            IDomainEventPublisher domainEventPublisher,
            IEventProcessorOutbox outbox)
        {
            _logger = logger;
            _domainEventPublisher = domainEventPublisher;
            _outbox = outbox;
            _waitCancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<bool> ProcessNextOutboxEventAsync()
        {
            var nextEvent = await _outbox.GetNextAndMarkProcessingAsync();
            var didPublish = false;
            
            if (nextEvent != null)
            {
                try
                {
                    if (nextEvent is IDomainEvent @nextDomainEvent)
                    {
                        await _domainEventPublisher.PublishAsync(@nextDomainEvent);
                    }
                    else if (nextEvent is IIntegrationEvent @nextIntegrationEvent)
                    {
                        throw new NotImplementedException("Use integration event publisher when we have it and publish the event using it here.");
                    }
                    else
                    {
                        throw new Exception("Can't publish event from outbox, the event type is unknown. This should never happen.");
                    }
                    
                    didPublish = true;
                }
                catch (Exception e)
                {
                    _logger.LogError($"Failed to publish outbox event: {e}");
                }
        
                if (didPublish)
                    await _outbox.RemoveEventAsync(nextEvent);
                else
                    await _outbox.MarkNotProcessingAsync(nextEvent);
            }

            return didPublish;
        }
        
        
        //
        // public Task StopWorkingOutboxAsync(CancellationToken cancellationToken)
        // {
        //     _waitCancellationTokenSource.Cancel();
        //     return Task.CompletedTask;
        // }
        //
        // // Helpers
        //
        // private Task<OutboxEvent> GetNextAsync()
        //     => _outbox.GetNextAsync(CancellationToken.None);
        //
        // private Task MarkAsNotPublishingAsync(OutboxEvent outboxEvent)
        //     => _outbox.MarkAsNotPublishingAsync(outboxEvent.Id, CancellationToken.None);
        //
        // private async Task PublishAsync(OutboxEvent outboxEvent)
        // {
        //     if (outboxEvent.IsDomainEvent)
        //         await _domainPublisher.FlushAsync(outboxEvent);
        //     else
        //         await _interchangePublisher.FlushAsync(outboxEvent);
        // }
        //
        // private async Task DeleteFromOutboxAsync(OutboxEvent outboxEvent)
        // {
        //     await _outbox.RemoveAsync(outboxEvent.Id, CancellationToken.None);
        // }
        //
        // private async Task AddToOutboxAsync(IEvent theEvent)
        // {
        //     await _outbox.AddAllAsync(
        //         theEvent.Header.ActionId, 
        //         new List<IEvent> { theEvent }, 
        //         CancellationToken.None);
        // }
        //
        // // IStartable
        //
        // public bool IsStarted { get; set; } = false;
        // public void Start(CancellationToken ct)
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task StartAsync(CancellationToken ct, bool blocking = false)
        // {
        //     IsStarted = true;
        //     await WorkOutboxAsync(ct);
        // }
        //
        // public void Stop(CancellationToken ct)
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public Task StopAsync(CancellationToken ct, bool blocking = false)
        // {
        //     IsStarted = false;
        // }
    }
}
