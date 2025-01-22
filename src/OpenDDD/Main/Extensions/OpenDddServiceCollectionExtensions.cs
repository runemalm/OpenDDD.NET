using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenDDD.Application;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Service;
using OpenDDD.Main.Attributes;
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

            // Auto-register domain services
            if (options!.AutoRegisterDomainServices)
            {
                RegisterDomainServices(services);
            }

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

        private static void RegisterDomainServices(IServiceCollection services)
        {
            // Get all types implementing IDomainService or its ancestor interfaces
            var domainServiceInterfaces = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => 
                    type.IsInterface && 
                    type != typeof(IDomainService) && 
                    typeof(IDomainService).IsAssignableFrom(type))
                .ToList();

            foreach (var interfaceType in domainServiceInterfaces)
            {
                // Determine the implementation type by removing the leading 'I' from the interface name
                var implementationTypeName = interfaceType.Name.Substring(1);
                var implementationType = interfaceType.Assembly.GetTypes()
                    .FirstOrDefault(type => type.Name.Equals(implementationTypeName, StringComparison.Ordinal) && interfaceType.IsAssignableFrom(type));

                if (implementationType != null)
                {
                    // Check for LifetimeAttribute
                    var lifetimeAttribute = implementationType.GetCustomAttribute<LifetimeAttribute>();
                    var lifetime = lifetimeAttribute?.Lifetime ?? ServiceLifetime.Transient;

                    // Register the interface and its implementation
                    services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));
                    Console.WriteLine($"Registered domain service: {interfaceType.Name} with implementation: {implementationType.Name} and lifetime: {lifetime}");
                }
                else
                {
                    Console.WriteLine($"Warning: No implementation found for domain service interface: {interfaceType.Name}");
                }
            }
        }

        private static void RegisterRepositories(IServiceCollection services, IConfiguration configuration)
        {
            // Get the desired implementation type from configuration
            var persistenceProvider = configuration.GetValue<string>("OpenDDD:PersistenceProvider") ?? "Postgres";

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
                               type.Name.StartsWith(persistenceProvider) && 
                               type.Name.EndsWith("Repository"))
                .ToList();

            foreach (var interfaceType in repositoryInterfaces)
            {
                // Find a matching implementation using the naming convention
                var implementationType = repositoryImplementations.FirstOrDefault(
                    impl => impl.Name.Equals($"{persistenceProvider}{interfaceType.Name.Substring(1)}", StringComparison.Ordinal));

                if (implementationType != null && interfaceType.IsAssignableFrom(implementationType))
                {
                    // Check for LifetimeAttribute
                    var lifetimeAttribute = implementationType.GetCustomAttribute<LifetimeAttribute>();
                    var lifetime = lifetimeAttribute?.Lifetime ?? ServiceLifetime.Transient;

                    // Register the interface and its implementation
                    services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));
                    Console.WriteLine($"Registered repository: {interfaceType.Name} with implementation: {implementationType.Name} and lifetime: {lifetime}");
                }
                else
                {
                    Console.WriteLine($"Warning: No implementation found for {interfaceType.Name} with prefix '{persistenceProvider}'");
                }
            }
        }
        
        private static void RegisterActions(IServiceCollection services)
        {
            var actionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    type.IsClass &&
                    !type.IsAbstract &&
                    type.GetInterfaces().Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IAction<,>)))
                .ToList();

            foreach (var actionType in actionTypes)
            {
                // Register the action type with transient lifetime
                services.AddTransient(actionType);
                Console.WriteLine($"Registered action: {actionType.Name} with lifetime: Transient");
            }
        }
        
        private static void RegisterServiceManager(IServiceCollection services)
        {
            services.AddSingleton<IOpenDddServiceManager, OpenDddServiceManager>();
        }
    }
}
