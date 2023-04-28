using System.Threading.Tasks;
using OpenDDD.Application;

namespace OpenDDD.Infrastructure.Services.Persistence
{
    public interface IPersistenceService
    {
	    void Start();
	    Task StartAsync();
	    void Stop();
	    Task StopAsync();
	    void EnsureDatabase();
	    Task EnsureDatabaseAsync();
	    Task StartTransactionAsync(ActionId actionId);
	    Task CommitTransactionAsync(ActionId actionId);
	    Task RollbackTransactionAsync(ActionId actionId);
	    IConnection GetConnection(ActionId actionId);
	    Task<IConnection> GetConnectionAsync(ActionId actionId);
	    void ReleaseConnection(ActionId actionId);
	    Task ReleaseConnectionAsync(ActionId actionId);
        IConnection OpenConnection();
        Task<IConnection> OpenConnectionAsync();
    }
}
