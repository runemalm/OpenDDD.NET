using System.Threading.Tasks;
using DDD.Domain;

namespace DDD.Infrastructure.Persistence
{
    public interface IPersistenceService
    {
	    Task StartTransactionAsync(ActionId actionId);
	    Task CommitTransactionAsync(ActionId actionId);
	    Task RollbackTransactionAsync(ActionId actionId);
        Task<IConnection> GetConnectionAsync(ActionId actionId);
        Task ReleaseConnectionAsync(ActionId actionId);
        Task<IConnection> OpenConnectionAsync();
    }
}
