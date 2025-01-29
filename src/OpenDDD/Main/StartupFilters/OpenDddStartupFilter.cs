using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using OpenDDD.Infrastructure.Persistence.EfCore.Startup;
using OpenDDD.Main.Options;

namespace OpenDDD.Main.StartupFilters
{
    public class OpenDddStartupFilter : IStartupFilter
    {
        private readonly EfCoreDatabaseInitializer _efCoreDatabaseInitializer;
        private readonly OpenDddOptions _options;

        public OpenDddStartupFilter(
            EfCoreDatabaseInitializer efCoreDatabaseInitializer,
            IOptions<OpenDddOptions> options)
        {
            _efCoreDatabaseInitializer = efCoreDatabaseInitializer;
            _options = options.Value;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                if (_options.PersistenceProvider.Equals("EfCore", StringComparison.OrdinalIgnoreCase))
                {
                    _efCoreDatabaseInitializer.InitializeDatabases();
                }

                next(app);
            };
        }
    }
}
