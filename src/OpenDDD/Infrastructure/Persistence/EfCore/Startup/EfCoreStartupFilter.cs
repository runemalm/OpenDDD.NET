using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.Persistence.EfCore.Startup
{
    public class EfCoreStartupFilter : IStartupFilter
    {
        private readonly EfCoreDatabaseInitializer _efCoreDatabaseInitializer;
        private readonly OpenDddOptions _options;

        public EfCoreStartupFilter(
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
                _efCoreDatabaseInitializer.InitializeDatabases();
                next(app);
            };
        }
    }
}
