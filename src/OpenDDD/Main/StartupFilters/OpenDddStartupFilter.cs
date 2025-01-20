using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDD.Main.Managers;

namespace OpenDDD.Main.StartupFilters
{
    public class OpenDddStartupFilter : IStartupFilter
    {
        private readonly ILogger<OpenDddStartupFilter> _logger;

        public OpenDddStartupFilter(ILogger<OpenDddStartupFilter> logger)
        {
            _logger = logger;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return async app =>
            {
                var serviceProvider = app.ApplicationServices;
                var serviceManager = serviceProvider.GetRequiredService<IOpenDddServiceManager>();
                var lifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();

                // Start OpenDDD services during application startup
                try
                {
                    await serviceManager.StartServicesAsync();
                    _logger.LogInformation("OpenDDD services started successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to start OpenDDD services. Application will terminate.");
                    throw;
                }

                // Stop OpenDDD services during application shutdown
                lifetime.ApplicationStopping.Register(async () =>
                {
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                        await serviceManager.StopServicesAsync(cts.Token);
                        _logger.LogInformation("OpenDDD services stopped successfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to stop OpenDDD services.");
                    }
                });

                next(app);
            };
        }
    }
}
