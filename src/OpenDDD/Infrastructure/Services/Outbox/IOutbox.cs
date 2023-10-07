using System.Threading.Tasks;
using OpenDDD.Domain.Model.Event;

namespace OpenDDD.Infrastructure.Services.Outbox
{
    public interface IOutbox
    {
        Task AddEventAsync(IEvent theEvent);
        Task RemoveEventAsync(IEvent theEvent);
        Task<IEvent?> GetNextAndMarkProcessingAsync();
        Task MarkNotProcessingAsync(IEvent theEvent);
        bool HasPublished(IEvent theEvent);
        Task<bool> HasPublishedAsync(IEvent theEvent);
        int GetEventCount();
    }
}
