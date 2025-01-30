namespace OpenDDD.Infrastructure.Persistence.UoW
{
    public interface IUnitOfWork : IDisposable
    {
        Task StartAsync(CancellationToken ct);
        Task CommitAsync(CancellationToken ct);
        Task RollbackAsync(CancellationToken ct);
    }
}
