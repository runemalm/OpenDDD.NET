using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenDDD.Main.Attributes;
using OpenDDD.Main.Interfaces;

namespace OpenDDD.Main.Managers
{
    public class OpenDddServiceManager : IOpenDddServiceManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OpenDddServiceManager> _logger;

        public OpenDddServiceManager(IServiceProvider serviceProvider, ILogger<OpenDddServiceManager> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartServicesAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var startables = services.GetServices<object>()
                .Where(service => typeof(IStartable).IsAssignableFrom(service.GetType()))
                .Cast<IStartable>()
                .GroupBy(startable =>
                {
                    var priorityAttribute = startable.GetType()
                        .GetCustomAttribute<StartPriorityAttribute>();
                    return priorityAttribute?.StartPriority ?? 0;
                })
                .OrderBy(group => group.Key); // Ascending priority order

            if (!startables.Any())
            {
                _logger.LogWarning("No IStartable services found to start.");
            }

            foreach (var group in startables)
            {
                _logger.LogInformation($"Starting services in priority group {group.Key}...");
                var tasks = group.Select(async startable =>
                {
                    try
                    {
                        _logger.LogInformation($"Starting {startable.GetType().Name}...");
                        await startable.StartAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error starting service {startable.GetType().Name}");
                    }
                });

                await Task.WhenAll(tasks);
            }

            if (startables.Any())
            {
                _logger.LogInformation("All OpenDDD services have been started.");
            }
        }

        public async Task StopServicesAsync(CancellationToken cancellationToken = default)
        {
            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var stoppables = services.GetServices<object>()
                .Where(service => typeof(IStoppable).IsAssignableFrom(service.GetType()))
                .Cast<IStoppable>()
                .GroupBy(stoppable =>
                {
                    var priorityAttribute = stoppable.GetType()
                        .GetCustomAttribute<StopPriorityAttribute>();
                    return priorityAttribute?.StopPriority ?? 0;
                })
                .OrderByDescending(group => group.Key); // Descending priority order

            if (!stoppables.Any())
            {
                _logger.LogWarning("No IStoppable services found to stop.");
            }

            foreach (var group in stoppables)
            {
                _logger.LogInformation($"Stopping services in priority group {group.Key}...");
                var tasks = group.Select(async stoppable =>
                {
                    try
                    {
                        _logger.LogInformation($"Stopping {stoppable.GetType().Name}...");
                        await stoppable.StopAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error stopping service {stoppable.GetType().Name}");
                    }
                });

                await Task.WhenAll(tasks);
            }

            if (stoppables.Any())
            {
                _logger.LogInformation("All OpenDDD services have been stopped.");
            }
        }

    }
}
