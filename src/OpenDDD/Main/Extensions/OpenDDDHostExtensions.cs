using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDD.Main.Managers;

namespace OpenDDD.Main.Extensions
{
    public static class OpenDDDHostExtensions
    {
        public static async Task UseOpenDddServiceLifetimeManagerAsync(this IHost host)
        {
            var serviceManager = host.Services.GetRequiredService<IOpenDddServiceManager>();

            // Start OpenDDD services
            await serviceManager.StartServicesAsync();

            // Stop OpenDDD services on application shutdown
            host.Services.GetRequiredService<IHostApplicationLifetime>()
                .ApplicationStopping.Register(async void () =>
                {
                    try
                    {
                        await serviceManager.StopServicesAsync();
                    }
                    catch (Exception ex)
                    {
                        var logger = host.Services.GetService<ILogger<OpenDddServiceManager>>();
                        logger?.LogError(ex, "Error while stopping OpenDDD services.");
                    }
                });
        }
    }
}
