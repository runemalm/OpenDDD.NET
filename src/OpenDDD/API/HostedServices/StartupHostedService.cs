using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Infrastructure.Persistence.EfCore.Seeders;

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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting database migration and seeding...");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<OpenDddDbContextBase>();

            if (dbContext == null)
            {
                _logger.LogWarning("No OpenDddDbContextBase found. Skipping database initialization.");
                _startupCompleted.SetResult(true); // Allow dependent services to start
                return;
            }

            _logger.LogInformation($"Applying migrations for {dbContext.GetType().Name}...");
            await dbContext.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Database is up-to-date.");

            var seeders = scope.ServiceProvider.GetServices<IEfCoreSeeder>().ToList();
            if (seeders.Any())
            {
                _logger.LogInformation($"Executing {seeders.Count} registered seeders...");
                foreach (var seeder in seeders)
                {
                    _logger.LogInformation($"Running {seeder.GetType().Name}...");
                    await seeder.ExecuteAsync(dbContext, cancellationToken);
                    _logger.LogInformation($"{seeder.GetType().Name} completed successfully.");
                }
            }
            else
            {
                _logger.LogInformation("No seeders registered. Skipping seeding.");
            }

            _logger.LogInformation("Database setup completed.");
            _startupCompleted.SetResult(true); // Allow other services to proceed
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
