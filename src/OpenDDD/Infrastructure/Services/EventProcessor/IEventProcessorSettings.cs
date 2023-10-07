using OpenDDD.NET;

namespace OpenDDD.Infrastructure.Services.EventProcessor
{
    public interface IEventProcessorSettings : ISettings
    {
        bool Enabled { get; }
    }
}
