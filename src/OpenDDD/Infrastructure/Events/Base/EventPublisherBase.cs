// using OpenDDD.Domain.Model;
//
// namespace OpenDDD.Infrastructure.Events.Base
// {
//     public abstract class EventPublisherBase<TEvent> where TEvent : IEvent
//     {
//         private readonly List<TEvent> _publishedEvents = new();
//
//         public async Task PublishAsync(TEvent @event, CancellationToken ct)
//         {
//             if (@event == null) throw new ArgumentNullException(nameof(@event));
//
//             _publishedEvents.Add(@event);
//             await Task.CompletedTask; // Simulate async operation if needed
//         }
//
//         public IReadOnlyList<TEvent> GetPublishedEvents() => _publishedEvents;
//     }
// }
