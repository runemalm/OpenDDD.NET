using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenDDD.Application;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Domain.Service;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Infrastructure.Events.Azure;
using OpenDDD.Infrastructure.Events.Base;
using OpenDDD.Infrastructure.Events.InMemory;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Infrastructure.Persistence.EfCore.Startup;
using OpenDDD.Infrastructure.Persistence.EfCore.UoW;
using OpenDDD.Infrastructure.Persistence.UoW;
using OpenDDD.Infrastructure.Repository.EfCore;
using OpenDDD.Infrastructure.Service;
using OpenDDD.Infrastructure.TransactionalOutbox;
using OpenDDD.Infrastructure.TransactionalOutbox.EfCore;
using OpenDDD.Main.Attributes;
using OpenDDD.Main.Options;
using OpenDDD.Main.StartupFilters;

namespace OpenDDD.Main.Extensions
{
    public static class OpenDddServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenDDD<TDbContext>(this IServiceCollection services,
            IConfiguration configuration,
            Action<OpenDddOptions>? configureOptions = null,
            Action<DbContextOptionsBuilder>? dbContextOptions = null,
            Action<IServiceCollection>? configureServices = null)
            where TDbContext : OpenDddDbContextBase
        {
            // Bind the OpenDDD options
            services.Configure<OpenDddOptions>(opt =>
            {
                configuration.GetSection("OpenDDD").Bind(opt);
                configureOptions?.Invoke(opt);
            });

            // Construct an OpenDdd options object
            var options = new OpenDddOptions();
            configuration.GetSection("OpenDDD").Bind(options);
            configureOptions?.Invoke(options);
            
            // Register the options object as singleton as well
            services.AddSingleton(options);

            // Validate options
            if (options.PersistenceProvider.ToLower() == "efcore")
            {
                if (options.EfCore.Database.ToLower() != "inmemory")
                {
                    if (string.IsNullOrWhiteSpace(options.EfCore.ConnectionString))
                    {
                        throw new InvalidOperationException("OpenDDD with EfCore requires a valid connection string. " +
                                                            "Please set 'OpenDDD:EfCore:ConnectionString'.");
                    }
                }
            }

            // Register services for EfCore
            if (options.PersistenceProvider.ToLower() == "efcore")
            {
                services.AddDbContext<TDbContext>((serviceProvider, optionsBuilder) =>
                {
                    var openDddOptions = serviceProvider.GetRequiredService<IOptions<OpenDddOptions>>().Value;

                    switch (openDddOptions.EfCore.Database.ToLower())
                    {
                        case "postgres":
                            optionsBuilder.UseNpgsql(openDddOptions.EfCore.ConnectionString);
                            break;
                        case "sqlserver":
                            optionsBuilder.UseSqlServer(openDddOptions.EfCore.ConnectionString);
                            break;
                        case "sqlite":
                            optionsBuilder.UseSqlite(openDddOptions.EfCore.ConnectionString);
                            break;
                        case "inmemory":
                            optionsBuilder.UseInMemoryDatabase("OpenDDD");
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported Database: {openDddOptions.EfCore.Database}");
                    }

                    dbContextOptions?.Invoke(optionsBuilder);
                });
            
                // Also register with the base class
                services.AddScoped<OpenDddDbContextBase>(sp => sp.GetRequiredService<TDbContext>());
                
                // Register outbox repository
                services.AddTransient<IOutboxRepository, EfCoreOutboxRepository>();
                
                // Register the database initializer
                services.AddSingleton<EfCoreDatabaseInitializer>();
            }

            // Register the unit-of-work
            services.AddScoped<IUnitOfWork>(sp =>
            {
                if (options.PersistenceProvider.ToLower() == "efcore")
                {
                    var dbContext = sp.GetRequiredService<TDbContext>();
                    return new EfCoreUnitOfWork(dbContext);
                }
                throw new Exception($"Unsupported PersistenceProvider: {options.PersistenceProvider}");
            });
            
            // Register event services
            if (options.MessagingProvider == "InMemory")
            {
                services.AddSingleton<IMessagingProvider, InMemoryMessagingProvider>();
            }
            else if (options.MessagingProvider == "AzureServiceBus")
            {
                services.AddSingleton<IMessagingProvider, AzureServiceBusProvider>();
            }
            
            // Register publishers
            services.AddScoped<IDomainPublisher, DomainPublisher>();
            services.AddScoped<IIntegrationPublisher, IntegrationPublisher>();

            // Auto-register repositories
            if (options.AutoRegister.Repositories)
            {
                if (options.PersistenceProvider.ToLower() == "efcore")
                {
                    RegisterEfCoreRepositories(services);
                }
            }

            // Auto-register domain services
            if (options.AutoRegister.DomainServices)
            {
                RegisterDomainServices(services);
            }

            // Auto-register actions
            if (options.AutoRegister.Actions)
            {
                RegisterActions(services);
            }
            
            if (options.AutoRegister.InfrastructureServices)
            {
                RegisterInfrastructureServices(services);
            }
            
            if (options.AutoRegister.EventListeners)
            {
                RegisterEventListeners(services, options);
            }
            
            // Register transactional outbox services
            services.AddHostedService<OutboxProcessor>();

            // Allow additional service configuration
            configureServices?.Invoke(services);

            // Register service manager and startup filter
            services.AddSingleton(services);
            services.AddSingleton<IStartupFilter, OpenDddStartupFilter>();

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
        
        private static void RegisterInfrastructureServices(IServiceCollection services)
        {
            // Get all non-abstract classes implementing IInfrastructureService
            var infrastructureServiceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => 
                    type.IsClass && 
                    !type.IsAbstract && 
                    typeof(IInfrastructureService).IsAssignableFrom(type))
                .ToList();

            // Group implementations by their interface (excluding IInfrastructureService itself)
            var groupedServices = infrastructureServiceTypes
                .SelectMany(impl => impl.GetInterfaces()
                    .Where(i => i != typeof(IInfrastructureService) && !i.IsGenericType)
                    .Select(i => new { Interface = i, Implementation = impl }))
                .GroupBy(x => x.Interface)
                .ToList();

            foreach (var group in groupedServices)
            {
                var serviceInterface = group.Key;
                var implementations = group.Select(x => x.Implementation).ToList();

                if (implementations.Count == 1)
                {
                    var implementationType = implementations.Single();

                    // Check for custom lifetime using LifetimeAttribute or default to Transient
                    var lifetimeAttribute = implementationType.GetCustomAttribute<LifetimeAttribute>();
                    var lifetime = lifetimeAttribute?.Lifetime ?? ServiceLifetime.Transient;

                    // Register the service
                    services.Add(new ServiceDescriptor(serviceInterface, implementationType, lifetime));
                    Console.WriteLine($"Registered infrastructure service: {serviceInterface.Name} with implementation: {implementationType.Name} and lifetime: {lifetime}");
                }
                else
                {
                    Console.WriteLine($"Skipping auto-registration for {serviceInterface.Name} due to multiple implementations: {string.Join(", ", implementations.Select(i => i.Name))}. Please register manually.");
                }
            }
        }

        private static void RegisterEventListeners(IServiceCollection services, OpenDddOptions options)
        {
            if (!options.AutoRegister.EventListeners) return;

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
