using System.Threading.Tasks;
using OpenDDD.Domain.Model.Event;

namespace OpenDDD.Infrastructure.Services.Outbox
{
    public interface IOutbox
    {
        Task AddEventAsync(IEvent theEvent);
        bool HasPublished(IEvent theEvent);
        Task<bool> HasPublishedAsync(IEvent theEvent);
    }
}
