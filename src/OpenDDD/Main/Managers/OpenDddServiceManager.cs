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
            // Access the IServiceCollection registered at startup
            var serviceCollection = _serviceProvider.GetRequiredService<IServiceCollection>();
            
            // Get all registered services implementing IStartable
            var startableTypes = serviceCollection
                .Where(descriptor => descriptor.ImplementationType != null && typeof(IStartable).IsAssignableFrom(descriptor.ImplementationType))
                .Select(descriptor => descriptor.ServiceType)
                .Distinct();

            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            // Resolve instances for the discovered types
            var startableServices = startableTypes
                .Select(type => services.GetService(type))
                .Where(instance => instance != null)
                .Cast<IStartable>()
                .GroupBy(instance =>
                {
                    var priorityAttribute = instance.GetType()
                        .GetCustomAttribute<StartPriorityAttribute>();
                    return priorityAttribute?.StartPriority ?? 10000;
                })
                .OrderBy(group => group.Key);

            if (!startableServices.Any())
            {
                _logger.LogWarning("No IStartable services found to start.");
                return;
            }

            foreach (var group in startableServices)
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

            _logger.LogInformation("All OpenDDD services have been started.");
        }

        public async Task StopServicesAsync(CancellationToken cancellationToken = default)
        {
            // Access the IServiceCollection registered at startup
            var serviceCollection = _serviceProvider.GetRequiredService<IServiceCollection>();

            // Get all registered services implementing IStoppable
            var stoppableTypes = serviceCollection
                .Where(descriptor => descriptor.ImplementationType != null && typeof(IStoppable).IsAssignableFrom(descriptor.ImplementationType))
                .Select(descriptor => descriptor.ServiceType)
                .Distinct();

            using var scope = _serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            // Resolve instances for the discovered types
            var stoppableServices = stoppableTypes
                .Select(type => services.GetService(type))
                .Where(instance => instance != null)
                .Cast<IStoppable>()
                .GroupBy(instance =>
                {
                    var priorityAttribute = instance.GetType()
                        .GetCustomAttribute<StopPriorityAttribute>();
                    return priorityAttribute?.StopPriority ?? 10000;
                })
                .OrderByDescending(group => group.Key); // Descending priority order

            if (!stoppableServices.Any())
            {
                _logger.LogWarning("No IStoppable services found to stop.");
                return;
            }

            foreach (var group in stoppableServices)
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

            _logger.LogInformation("All OpenDDD services have been stopped.");
        }
    }
}
