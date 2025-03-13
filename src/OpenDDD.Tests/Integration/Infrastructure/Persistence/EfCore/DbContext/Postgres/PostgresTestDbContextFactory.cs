using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;
using OpenDDD.API.Options;

namespace OpenDDD.Tests.Integration.Infrastructure.Persistence.EfCore.DbContext.Postgres
{
    public class PostgresTestDbContextFactory : IDesignTimeDbContextFactory<PostgresTestDbContext>
    {
        public PostgresTestDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PostgresTestDbContext>()
                .UseNpgsql("Host=localhost;Port=5432;Database=testdb;Username=testuser;Password=testpassword")
                .EnableSensitiveDataLogging();

            var openDddOptions = new OpenDddOptions();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<PostgresTestDbContext>();

            return new PostgresTestDbContext(optionsBuilder.Options, openDddOptions, logger);
        }
    }
}
