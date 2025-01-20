using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenDDD.Main.Attributes;
using OpenDDD.Main.Interfaces;

namespace OpenDDD.Main.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static async Task StartOpenDddServicesAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            // Resolve the logger
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("OpenDDD.Startable");

            var startables = services.GetServices<object>()
                .Where(service => typeof(IStartable).IsAssignableFrom(service.GetType()))
                .Cast<IStartable>()
                .OrderBy(startable =>
                {
                    var priorityAttribute = startable.GetType()
                        .GetCustomAttribute<StartPriorityAttribute>();
                    return priorityAttribute?.StartPriority ?? 0;
                });

            foreach (var startable in startables)
            {
                try
                {
                    logger.LogInformation($"Starting {startable.GetType().Name}...");
                    await startable.StartAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error starting service {startable.GetType().Name}");
                }
            }

            logger.LogInformation("All services have been started.");
        }

        public static async Task StopOpenDddServicesAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            // Resolve the logger
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("OpenDDD.Stoppable");

            var stoppables = services.GetServices<object>()
                .Where(service => typeof(IStoppable).IsAssignableFrom(service.GetType()))
                .Cast<IStoppable>()
                .OrderByDescending(stoppable =>
                {
                    var priorityAttribute = stoppable.GetType()
                        .GetCustomAttribute<StopPriorityAttribute>();
                    return priorityAttribute?.StopPriority ?? 0;
                });

            foreach (var stoppable in stoppables)
            {
                try
                {
                    logger.LogInformation($"Stopping {stoppable.GetType().Name}...");
                    await stoppable.StopAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error stopping service {stoppable.GetType().Name}");
                }
            }

            logger.LogInformation("All services have been stopped.");
        }
    }
}
