using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.Database
{
    public interface ITransactionalDatabaseConnection
    {
        Task StartTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
