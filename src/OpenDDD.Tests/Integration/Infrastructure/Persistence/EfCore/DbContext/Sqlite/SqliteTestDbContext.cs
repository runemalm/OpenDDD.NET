using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenDDD.API.Options;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Tests.Base.Domain.Model;

namespace OpenDDD.Tests.Integration.Infrastructure.Persistence.EfCore.DbContext.Sqlite
{
    public class SqliteTestDbContext : OpenDddDbContextBase
    {
        public SqliteTestDbContext(DbContextOptions<SqliteTestDbContext> options, OpenDddOptions openDddOptions, ILogger<OpenDddDbContextBase> logger)
            : base(options, openDddOptions, logger)
        {
        }

        public DbSet<TestAggregateRoot> TestAggregates { get; set; } = null!;
    }
}
