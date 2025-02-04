using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenDDD.API.HostedServices;
using OpenDDD.API.Options;

namespace OpenDDD.API.StartupFilters
{
    public class StartupFilter : IStartupFilter
    {
        private readonly StartupHostedService _startupService;
        private readonly ILogger<StartupFilter> _logger;
        private readonly OpenDddOptions _options;

        public StartupFilter(
            StartupHostedService startupService,
            ILogger<StartupFilter> logger,
            IOptions<OpenDddOptions> options)
        {
            _startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));
            _logger = logger;
            _options = options.Value;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                _logger.LogInformation("Startup Filter running.");

                _logger.LogInformation("Waiting for startup service to complete before starting kestrel...");
                _startupService.StartupCompleted.GetAwaiter().GetResult();
                _logger.LogInformation("Startup service completed. Continuing kestrel startup...");

                next(app);
            };
        }
    }
}
