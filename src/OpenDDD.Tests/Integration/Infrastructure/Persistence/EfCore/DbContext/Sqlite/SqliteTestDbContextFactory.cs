using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using OpenDDD.API.Options;

namespace OpenDDD.Tests.Integration.Infrastructure.Persistence.EfCore.DbContext.Sqlite
{
    public class SqliteTestDbContextFactory : IDesignTimeDbContextFactory<SqliteTestDbContext>
    {
        public SqliteTestDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqliteTestDbContext>()
                .UseSqlite("DataSource=:memory:")
                .EnableSensitiveDataLogging();

            var openDddOptions = new OpenDddOptions();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<SqliteTestDbContext>();

            return new SqliteTestDbContext(optionsBuilder.Options, openDddOptions, logger);
        }
    }
}
