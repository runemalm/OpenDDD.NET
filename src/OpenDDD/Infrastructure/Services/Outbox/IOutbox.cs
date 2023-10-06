using System.Threading.Tasks;
using OpenDDD.Domain.Model.Event;

namespace OpenDDD.Infrastructure.Services.Outbox
{
    public interface IOutbox
    {
        Task AddEventAsync(IEvent theEvent);
        Task<IEvent?> NextEventAsync();
        bool HasPublished(IEvent theEvent);
        Task<bool> HasPublishedAsync(IEvent theEvent);
        int GetEventCount();
    }
}
