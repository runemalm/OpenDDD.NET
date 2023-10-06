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
        
        public async Task ProcessNextOutboxEventAsync()
        {
            var nextEvent = await _outbox.NextEventAsync();
            
            if (nextEvent != null)
            {
                if (nextEvent is IDomainEvent @event)
                {
                    await _domainEventPublisher.PublishAsync(@event);
                }
                else if (nextEvent.Header.EventType == EventType.IntegrationEvent)
                {
                    throw new NotImplementedException("Use integration event publisher when we have it and publish the event using it here.");
                }
                else
                {
                    throw new Exception("Can't publish event from outbox, the event type is unknown. This should never happen.");
                }
            }
        }

        // public async Task WorkOutboxAsync(CancellationToken stoppingToken)
        // {
        //     while (!stoppingToken.IsCancellationRequested && 
        //            !_waitCancellationTokenSource.Token.IsCancellationRequested &&
        //            !_isStarted)
        //     {
        //         try
        //         {
        //             await Task.Delay(500, _waitCancellationTokenSource.Token);
        //         }
        //         catch (TaskCanceledException e)
        //         {
        //             _logger.Log("Wait cancellationtoken was canceled while publisherservice was waiting to be started.", LogLevel.Debug);
        //         }
        //     }
        //
        //     while (!stoppingToken.IsCancellationRequested && 
        //            !_waitCancellationTokenSource.Token.IsCancellationRequested &&
        //            _isStarted)
        //     {
        //         var outboxEvent = await GetNextAsync();
        //
        //         if (outboxEvent != null)
        //         {
        //             var didPublish = true;
        //
        //             try
        //             {
        //                 await PublishAsync(outboxEvent);
        //             }
        //             catch (Exception e)
        //             {
        //                 _logger.Log($"Exception when publishing in publisher: {e}", LogLevel.Error);
        //                 didPublish = false;
        //             }
        //
        //             if (didPublish)
        //                 await DeleteFromOutboxAsync(outboxEvent);
        //             else
        //                 await MarkAsNotPublishingAsync(outboxEvent);
        //         }
        //
        //         if (outboxEvent == null)
        //         {
        //             try
        //             {
        //                 await Task.Delay(1000, _waitCancellationTokenSource.Token);
        //             }
        //             catch (TaskCanceledException e)
        //             {
        //                 _logger.Log("Wait cancellationtoken was canceled while publisherservice was waiting for next work cycle in started mode.", LogLevel.Debug);
        //             }
        //         }
        //     }
        // }
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
