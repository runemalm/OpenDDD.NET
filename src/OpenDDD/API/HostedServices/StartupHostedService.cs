using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Infrastructure.Persistence.EfCore.Seeders;
using OpenDDD.API.Options;
using OpenDDD.API.Extensions;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
using OpenDDD.Infrastructure.Persistence.OpenDdd.Seeders.Postgres;
using Npgsql;

namespace OpenDDD.API.HostedServices
{
    public class StartupHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StartupHostedService> _logger;
        private TaskCompletionSource<bool> _startupCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public StartupHostedService(IServiceProvider serviceProvider, ILogger<StartupHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartupCompleted => _startupCompleted.Task;

        public async Task StartAsync(CancellationToken ct)
        {
            _logger.LogInformation("Starting database initialization...");

            using var scope = _serviceProvider.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<OpenDddOptions>();

            if (options.PersistenceProvider.Equals("efcore", StringComparison.OrdinalIgnoreCase))
            {
                await InitializeEfCore(scope, ct);
            }
            else if (options.PersistenceProvider.Equals("openddd", StringComparison.OrdinalIgnoreCase))
            {
                if (options.DatabaseProvider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
                {
                    await InitializePostgres(scope, ct);
                } 
                else if (options.DatabaseProvider.Equals("inmemory", StringComparison.OrdinalIgnoreCase))
                {
                    await InitializeInMemory(scope, ct);
                }
            }

            _logger.LogInformation("Database initialization completed.");
            _startupCompleted.SetResult(true);
        }

        private async Task InitializeEfCore(IServiceScope scope, CancellationToken ct)
        {
            // Migrate
            _logger.LogInformation("Applying EF Core migrations...");

            var dbContext = scope.ServiceProvider.GetService<OpenDddDbContextBase>();
            if (dbContext == null)
            {
                _logger.LogWarning("No OpenDddDbContextBase found. Skipping EF Core migration.");
                return;
            }

            await dbContext.Database.MigrateAsync(ct);
            _logger.LogInformation("EF Core migrations applied.");

            // Seed
            var seeders = scope.ServiceProvider.GetServices<IEfCoreSeeder>().ToList();
            if (seeders.Any())
            {
                _logger.LogInformation($"Executing {seeders.Count} EF Core seeders...");
                foreach (var seeder in seeders)
                {
                    _logger.LogInformation($"Running {seeder.GetType().Name}...");
                    await seeder.ExecuteAsync(dbContext, ct);
                    _logger.LogInformation($"{seeder.GetType().Name} completed successfully.");
                }
            }
            else
            {
                _logger.LogInformation("No EF Core seeders found.");
            }
        }

        private async Task InitializePostgres(IServiceScope scope, CancellationToken ct)
        {
            // Ensure tables
            _logger.LogInformation("Ensuring PostgreSQL tables exist...");

            var databaseSession = scope.ServiceProvider.GetRequiredService<PostgresDatabaseSession>();

            await using var connection = databaseSession.Connection;
            await connection.OpenAsync(ct);
            await using var transaction = await connection.BeginTransactionAsync(ct);

            await EnsurePostgresOutboxTableAsync(connection, transaction, ct);
            await EnsurePostgresAggregateTablesAsync(connection, transaction, ct);
            
            await transaction.CommitAsync(ct);
            
            // Execute seeders
            var seeders = scope.ServiceProvider.GetServices<IPostgresOpenDddSeeder>().ToList();
            if (seeders.Any())
            {
                _logger.LogInformation($"Executing {seeders.Count} PostgreSQL OpenDDD seeders...");
                foreach (var seeder in seeders)
                {
                    _logger.LogInformation($"Running {seeder.GetType().Name}...");
                    await seeder.ExecuteAsync(databaseSession, ct);
                    _logger.LogInformation($"{seeder.GetType().Name} completed successfully.");
                }
            }
            else
            {
                _logger.LogInformation("No PostgreSQL OpenDDD seeders found.");
            }
        }
        
        private async Task InitializeInMemory(IServiceScope scope, CancellationToken ct)
        {
            
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

        private async Task EnsurePostgresOutboxTableAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken ct)
        {
            // Ensure Outbox Table Exists
            const string outboxTable = "outbox_entries";
            _logger.LogInformation($"Checking outbox table: {outboxTable}");

            var outboxTableExistsQuery = $@"
                SELECT EXISTS (
                    SELECT FROM information_schema.tables 
                    WHERE table_name = '{outboxTable}'
                );";

            await using var checkOutboxCmd = new NpgsqlCommand(outboxTableExistsQuery, connection, transaction);
            var outboxTableExists = (bool)(await checkOutboxCmd.ExecuteScalarAsync(ct) ?? false);

            if (!outboxTableExists)
            {
                _logger.LogInformation($"Creating outbox table: {outboxTable}");

                var createOutboxTableQuery = $@"
                    CREATE TABLE {outboxTable} (
                        id UUID PRIMARY KEY,
                        event_type TEXT NOT NULL,
                        event_name TEXT NOT NULL,
                        payload JSONB NOT NULL,
                        created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                        processed_at TIMESTAMP NULL
                    );
                    CREATE INDEX idx_{outboxTable}_processed ON {outboxTable} (processed_at);";

                await using var createOutboxCmd = new NpgsqlCommand(createOutboxTableQuery, connection, transaction);
                await createOutboxCmd.ExecuteNonQueryAsync(ct);

                _logger.LogInformation($"Outbox table {outboxTable} created.");
            }
        }

        private async Task EnsurePostgresAggregateTablesAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, CancellationToken ct)
        {
            var aggregateTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && InheritsFromAggregateRoot(t))
                .ToList();

            foreach (var aggregateType in aggregateTypes)
            {
                var tableName = aggregateType.Name.ToLower().Pluralize();
                _logger.LogInformation($"Checking table: {tableName}");

                var tableExistsQuery = $@"
                    SELECT EXISTS (
                        SELECT FROM information_schema.tables 
                        WHERE table_name = '{tableName}'
                    );";

                await using var checkCmd = new NpgsqlCommand(tableExistsQuery, connection, transaction);
                var tableExists = (bool)(await checkCmd.ExecuteScalarAsync(ct) ?? false);

                if (!tableExists)
                {
                    _logger.LogInformation($"Creating table: {tableName}");

                    var createTableQuery = $@"
                        CREATE TABLE {tableName} (
                            id UUID PRIMARY KEY,
                            data JSONB NOT NULL
                        );
                        CREATE INDEX idx_{tableName}_data ON {tableName} USING GIN (data);";

                    await using var createCmd = new NpgsqlCommand(createTableQuery, connection, transaction);
                    await createCmd.ExecuteNonQueryAsync(ct);

                    _logger.LogInformation($"Table {tableName} created.");
                }
            }
        }

        private static bool InheritsFromAggregateRoot(Type type)
        {
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AggregateRootBase<>))
                    return true;

                type = type.BaseType!;
            }
            return false;
        }
    }
}
