using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.Database
{
    public interface ITransactionalDatabase
    {
        Task StartTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
