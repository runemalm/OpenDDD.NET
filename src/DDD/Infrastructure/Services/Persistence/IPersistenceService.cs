using System.Threading.Tasks;
using DDD.Application;

namespace DDD.Infrastructure.Services.Persistence
{
    public interface IPersistenceService
    {
	    Task StartTransactionAsync(ActionId actionId);
	    Task CommitTransactionAsync(ActionId actionId);
	    Task RollbackTransactionAsync(ActionId actionId);
        Task<IConnection> GetConnectionAsync(ActionId actionId);
        Task ReleaseConnectionAsync(ActionId actionId);
        Task<IConnection> OpenConnectionAsync();
        Task StartAsync();
        Task StopAsync();
        Task EnsureDatabaseAsync();
    }
}
