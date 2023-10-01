using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.Database
{
    public interface IDatabase
    {
        void Truncate();
        Task TruncateAsync();
    }
}
