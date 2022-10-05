using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain;
using DDD.Domain.Model;

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
    }
}
