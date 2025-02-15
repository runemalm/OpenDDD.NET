using OpenDDD.Infrastructure.Persistence.EfCore.Base;

namespace OpenDDD.Infrastructure.Persistence.EfCore.Seeders
{
    public interface IEfCoreSeeder
    {
        Task ExecuteAsync(OpenDddDbContextBase dbContext, CancellationToken ct);
    }
}
