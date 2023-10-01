using System.Threading;
using System.Threading.Tasks;

namespace OpenDDD.NET
{
    public interface IStartable
    {
        bool IsStarted { get; set; }
        
        void Start(CancellationToken ct);
        Task StartAsync(CancellationToken ct, bool blocking = false);
        void Stop(CancellationToken ct);
        Task StopAsync(CancellationToken ct, bool blocking = false);
    }
}
