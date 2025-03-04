using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenDDD.API.Options;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Tests.Domain.Model;

namespace OpenDDD.Tests.Integration.Infrastructure.Persistence.EfCore.DbContext.Postgres
{
    public class PostgresTestDbContext : OpenDddDbContextBase
    {
        public PostgresTestDbContext(DbContextOptions<PostgresTestDbContext> options, OpenDddOptions openDddOptions, ILogger<OpenDddDbContextBase> logger)
            : base(options, openDddOptions, logger)
        {
        }

        public DbSet<TestAggregateRoot> TestAggregates { get; set; } = null!;
    }
}
