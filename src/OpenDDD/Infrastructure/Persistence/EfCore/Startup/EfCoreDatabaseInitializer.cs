using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;

namespace OpenDDD.Infrastructure.Persistence.EfCore.Startup
{
    public class EfCoreDatabaseInitializer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EfCoreDatabaseInitializer> _logger;

        public EfCoreDatabaseInitializer(IServiceProvider serviceProvider, ILogger<EfCoreDatabaseInitializer> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void InitializeDatabases()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContexts = scope.ServiceProvider.GetServices<OpenDddDbContextBase>().ToList();

            foreach (var dbContext in dbContexts)
            {
                _logger.LogInformation($"Ensuring database is created and migrations are applied for {dbContext.GetType().Name}...");
                dbContext.Database.Migrate();
                _logger.LogInformation($"Database ready for {dbContext.GetType().Name}.");
            }
        }
    }
}
