using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenDDD.Application;
using OpenDDD.Domain.Model;
using OpenDDD.Main.Managers;
using OpenDDD.Main.Options;
using OpenDDD.Main.StartupFilters;

namespace OpenDDD.Main.Extensions
{
    public static class OpenDddServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenDDD(this IServiceCollection services,
            IConfiguration configuration,
            Action<OpenDddOptions>? configureOptions = null,
            Action<IServiceCollection>? configureServices = null)
        {
            // Configure OpenDddOptions
            services.Configure<OpenDddOptions>(options =>
            {
                configuration.GetSection("OpenDDD").Bind(options);
                configureOptions?.Invoke(options);

                if (string.IsNullOrWhiteSpace(options.ConnectionString))
                {
                    options.ConnectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(options.ConnectionString))
                {
                    throw new InvalidOperationException("OpenDDD requires a valid connection string. Please set " +
                                                        "'OpenDDD:ConnectionString' or " +
                                                        "'ConnectionStrings:DefaultConnection'.");
                }
            });
            
            var options = configuration.GetSection("OpenDDD").Get<OpenDddOptions>();

            // Auto-register repositories
            if (options.AutoRegisterRepositories)
            {
                RegisterRepositories(services, configuration);
            }
            
            // Auto-register actions
            if (options.AutoRegisterActions)
            {
                RegisterActions(services);
            }

            // Allow additional service configuration
            configureServices?.Invoke(services);

            // Register the service manager and startup filter
            RegisterServiceManager(services);
            services.AddSingleton<IStartupFilter, OpenDddStartupFilter>();
            
            // Register the service collection itself so we can use it to start services in the service manager
            services.AddSingleton(services);

            return services;
        }

        private static void RegisterActions(IServiceCollection services)
        {
            var actionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IAction<,>).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            foreach (var actionType in actionTypes)
            {
                services.AddTransient(actionType);
                Console.WriteLine($"Registered action: {actionType.Name}");
            }
        }

        private static void RegisterRepositories(IServiceCollection services, IConfiguration configuration)
        {
            // Get the desired implementation type from configuration
            var repositoryImplementation = configuration.GetValue<string>("OpenDDD:RepositoryImplementation") ?? "Postgres";

            // Find all repository interfaces
            var repositoryInterfaces = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => 
                    type.IsInterface &&
                    type.Name.EndsWith("Repository") &&
                    type.GetInterfaces().Any(i => 
                        i.IsGenericType && 
                        i.GetGenericTypeDefinition() == typeof(IRepository<,>)
                    )
                )
                .ToList();

            // Find all repository implementations
            var repositoryImplementations = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsInterface && !type.IsAbstract && 
                               type.Name.StartsWith(repositoryImplementation) && 
                               type.Name.EndsWith("Repository"))
                .ToList();

            foreach (var interfaceType in repositoryInterfaces)
            {
                // Find a matching implementation using the naming convention
                var implementationType = repositoryImplementations.FirstOrDefault(
                    impl => impl.Name.Equals($"{repositoryImplementation}{interfaceType.Name.Substring(1)}", StringComparison.Ordinal));

                if (implementationType != null && interfaceType.IsAssignableFrom(implementationType))
                {
                    // Register the interface and its implementation
                    services.AddTransient(interfaceType, implementationType);
                    Console.WriteLine($"Registered {implementationType.Name} for {interfaceType.Name}");
                }
                else
                {
                    Console.WriteLine($"Warning: No implementation found for {interfaceType.Name} with prefix '{repositoryImplementation}'");
                }
            }
        }
        
        private static void RegisterServiceManager(IServiceCollection services)
        {
            services.AddSingleton<IOpenDddServiceManager, OpenDddServiceManager>();
        }
    }
}
