using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using OpenDDD.Infrastructure.Persistence.EfCore.Startup;

namespace OpenDDD.Main.StartupFilters
{
    public class OpenDddStartupFilter : IStartupFilter
    {
        private readonly EfCoreDatabaseInitializer _efCoreDatabaseInitializer;

        public OpenDddStartupFilter(EfCoreDatabaseInitializer efCoreDatabaseInitializer)
        {
            _efCoreDatabaseInitializer = efCoreDatabaseInitializer;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                _efCoreDatabaseInitializer.InitializeDatabases();
                next(app);
            };
        }
    }
}
