using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenDDD.Main.Managers;

namespace OpenDDD.Main.Extensions
{
    public static class OpenDddHostExtensions
    {
        public static async Task UseOpenDddServicesAsync(this IHost host)
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
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                        await serviceManager.StopServicesAsync(cts.Token);
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
