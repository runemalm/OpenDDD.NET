using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenDDD.API.Attributes;
using OpenDDD.API.HostedServices;
using OpenDDD.API.Options;
using OpenDDD.API.StartupFilters;
using OpenDDD.Application;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Domain.Service;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Infrastructure.Events.Azure;
using OpenDDD.Infrastructure.Events.Base;
using OpenDDD.Infrastructure.Events.InMemory;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Infrastructure.Persistence.EfCore.UoW;
using OpenDDD.Infrastructure.Persistence.UoW;
using OpenDDD.Infrastructure.Repository.EfCore;
using OpenDDD.Infrastructure.Service;
using OpenDDD.Infrastructure.TransactionalOutbox;
using OpenDDD.Infrastructure.TransactionalOutbox.EfCore;

namespace OpenDDD.API.Extensions
{
    public static class OpenDddServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenDDD<TDbContext>(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<OpenDddOptions>? configureOptions = null,
            Action<IServiceCollection>? configureServices = null)
            where TDbContext : OpenDddDbContextBase
        {
            services.AddDbContext<TDbContext>((serviceProvider, optionsBuilder) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<OpenDddOptions>>().Value;

                switch (options.EfCore.Database.ToLower())
                {
                    case "postgres":
                        optionsBuilder.UseNpgsql(options.EfCore.ConnectionString);
                        break;
                    case "sqlserver":
                        optionsBuilder.UseSqlServer(options.EfCore.ConnectionString);
                        break;
                    case "sqlite":
                        optionsBuilder.UseSqlite(options.EfCore.ConnectionString);
                        break;
                    case "inmemory":
                        optionsBuilder.UseInMemoryDatabase("OpenDDD");
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported Database: {options.EfCore.Database}");
                }
            });
            
            services.AddScoped<OpenDddDbContextBase>(sp => sp.GetRequiredService<TDbContext>());

            return AddOpenDDD(services, configuration, configureOptions, configureServices);
        }

        public static IServiceCollection AddOpenDDD(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<OpenDddOptions>? configureOptions = null,
            Action<IServiceCollection>? configureServices = null)
        {
            var options = services.AddAndResolveOptions(configuration, configureOptions);

            services.AddPersistence(options);
            services.AddMessaging(options);
            services.AddTransactionalOutbox();
            services.AddPublishing();
            services.AddStartup();

            if (options.AutoRegister.Repositories) RegisterEfCoreRepositories(services);
            if (options.AutoRegister.DomainServices) RegisterDomainServices(services);
            if (options.AutoRegister.InfrastructureServices) RegisterInfrastructureServices(services);
            if (options.AutoRegister.Actions) RegisterActions(services);
            if (options.AutoRegister.EventListeners) RegisterEventListeners(services);
            
            configureServices?.Invoke(services);

            return services;
        }

        private static OpenDddOptions AddAndResolveOptions(this IServiceCollection services,
            IConfiguration configuration, Action<OpenDddOptions>? configureOptions)
        {
            services.Configure<OpenDddOptions>(options =>
            {
                configuration.GetSection("OpenDDD").Bind(options);
                configureOptions?.Invoke(options);
            });
            
            var options = new OpenDddOptions();
            configuration.GetSection("OpenDDD").Bind(options);
            configureOptions?.Invoke(options);
            
            services.AddSingleton(sp => sp.GetRequiredService<IOptions<OpenDddOptions>>().Value);

            return options;
        }
        
        private static void AddPersistence(this IServiceCollection services, OpenDddOptions options)
        {
            switch (options.PersistenceProvider.ToLower())
            {
                case "efcore":
                    services.AddEfCore();
                    break;
                default:
                    throw new Exception($"Unsupported PersistenceProvider: {options.PersistenceProvider}");
            }
        }
        
        private static void AddMessaging(this IServiceCollection services, OpenDddOptions options)
        {
            switch (options.MessagingProvider.ToLower())
            {
                case "inmemory":
                    services.AddInMemoryMessaging();
                    break;
                case "azureservicebus":
                    services.AddAzureServiceBus();
                    break;
                default:
                    throw new Exception($"Unsupported MessagingProvider: {options.MessagingProvider}");
            }
        }
        
        private static void AddTransactionalOutbox(this IServiceCollection services)
        {
            services.AddHostedService<OutboxProcessor>();
        }
        
        private static void AddStartup(this IServiceCollection services)
        {
            services.AddSingleton<StartupHostedService>();
            services.AddHostedService(sp => sp.GetRequiredService<StartupHostedService>());
            services.AddSingleton<IStartupFilter, StartupFilter>();
        }

        private static void AddPublishing(this IServiceCollection services)
        {
            services.AddScoped<IDomainPublisher, DomainPublisher>();
            services.AddScoped<IIntegrationPublisher, IntegrationPublisher>();
        }

        private static void AddEfCore(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
            services.AddTransient<IOutboxRepository, EfCoreOutboxRepository>();
        }

        private static void AddAzureServiceBus(this IServiceCollection services)
        {
            services.AddSingleton<IMessagingProvider, AzureServiceBusMessagingProvider>();
        }
        
        private static void AddInMemoryMessaging(this IServiceCollection services)
        {
            services.AddSingleton<IMessagingProvider, InMemoryMessagingProvider>();
        }

        private static void RegisterDomainServices(IServiceCollection services)
        {
            RegisterServicesByInterface<IDomainService>(services, "domain service");
        }

        private static void RegisterInfrastructureServices(IServiceCollection services)
        {
            RegisterServicesByInterface<IInfrastructureService>(services, "infrastructure service");
        }

        private static void RegisterServicesByInterface<TServiceMarker>(IServiceCollection services, string serviceTypeName)
        {
            // Get all interfaces that implement the marker interface (excluding the marker itself)
            var serviceInterfaces = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    type.IsInterface &&
                    type != typeof(TServiceMarker) &&
                    typeof(TServiceMarker).IsAssignableFrom(type))
                .ToList();

            foreach (var interfaceType in serviceInterfaces)
            {
                // Determine the implementation type by removing the leading 'I' from the interface name
                var implementationTypeName = interfaceType.Name.Substring(1);
                var implementationType = interfaceType.Assembly.GetTypes()
                    .FirstOrDefault(type => type.Name.Equals(implementationTypeName, StringComparison.Ordinal) &&
                                            interfaceType.IsAssignableFrom(type));

                if (implementationType != null)
                {
                    // Check for LifetimeAttribute
                    var lifetimeAttribute = implementationType.GetCustomAttribute<LifetimeAttribute>();
                    var lifetime = lifetimeAttribute?.Lifetime ?? ServiceLifetime.Transient;

                    // Register the interface and its implementation
                    services.Add(new ServiceDescriptor(interfaceType, implementationType, lifetime));
                    Console.WriteLine($"Registered {serviceTypeName}: {interfaceType.Name} with implementation: {implementationType.Name} and lifetime: {lifetime}");
                }
                else
                {
                    Console.WriteLine($"Warning: No implementation found for {serviceTypeName} interface: {interfaceType.Name}");
                }
            }
        }

        private static void RegisterEfCoreRepositories(IServiceCollection services)
        {
            var aggregateRootType = typeof(AggregateRootBase<>);

            // Get all aggregate root types
            var aggregateTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract &&
                            t.BaseType != null &&
                            t.BaseType.IsGenericType &&
                            t.BaseType.GetGenericTypeDefinition() == aggregateRootType)
                .ToList();

            foreach (var aggregateType in aggregateTypes)
            {
                var idType = aggregateType.BaseType!.GetGenericArguments()[0];
                var repositoryInterfaceType = typeof(IRepository<,>).MakeGenericType(aggregateType, idType);

                // Find custom repository interfaces extending IRepository<,>
                var customRepositoryInterface = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .FirstOrDefault(t => t.IsInterface &&
                                         repositoryInterfaceType.IsAssignableFrom(t));

                var customRepositoryImplementation = customRepositoryInterface != null
                    ? AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(assembly => assembly.GetTypes())
                        .FirstOrDefault(t => !t.IsInterface &&
                                             !t.IsAbstract &&
                                             customRepositoryInterface.IsAssignableFrom(t))
                    : null;

                if (customRepositoryInterface != null && customRepositoryImplementation != null)
                {
                    // Register custom repository
                    services.AddTransient(customRepositoryInterface, customRepositoryImplementation);
                    Console.WriteLine($"Registered custom repository: {GetReadableTypeName(customRepositoryInterface)} with implementation: {GetReadableTypeName(customRepositoryImplementation)}.");
                }
                else
                {
                    // Fallback to default repository
                    var defaultRepositoryImplementationType = typeof(EfCoreRepository<,>).MakeGenericType(aggregateType, idType);
                    services.AddTransient(repositoryInterfaceType, defaultRepositoryImplementationType);
                    Console.WriteLine($"Registered default repository: {GetReadableTypeName(repositoryInterfaceType)} with implementation: {GetReadableTypeName(defaultRepositoryImplementationType)}.");
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
        
        private static void RegisterEventListeners(IServiceCollection services)
        {
            // Find all classes deriving from EventListenerBase<,>
            var listenerTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    type.IsClass &&
                    !type.IsAbstract &&
                    IsDerivedFromGenericType(type, typeof(EventListenerBase<,>)))
                .ToList();

            foreach (var listenerType in listenerTypes)
            {
                // Register the listener as a hosted service
                var addHostedServiceMethod = typeof(ServiceCollectionHostedServiceExtensions)
                    .GetMethod("AddHostedService", new[] { typeof(IServiceCollection) })
                    ?.MakeGenericMethod(listenerType);

                if (addHostedServiceMethod != null)
                {
                    addHostedServiceMethod.Invoke(null, new object[] { services });
                    Console.WriteLine($"Registered event listener: {listenerType.Name}");
                }
            }
        }

        private static bool IsDerivedFromGenericType(Type type, Type genericType)
        {
            while (type != null && type != typeof(object))
            {
                var currentType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (currentType == genericType)
                {
                    return true;
                }
                type = type.BaseType!;
            }
            return false;
        }
        
        private static string GetReadableTypeName(Type type)
        {
            if (!type.IsGenericType)
                return type.Name;

            var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetReadableTypeName));
            return $"{genericTypeName}<{genericArgs}>";
        }
    }
}
