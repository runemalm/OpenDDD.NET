using System.Reflection;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Confluent.Kafka;
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
using OpenDDD.Infrastructure.Events.Kafka;
using OpenDDD.Infrastructure.Events.Kafka.Factories;
using OpenDDD.Infrastructure.Events.RabbitMq;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Infrastructure.Persistence.EfCore.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.EfCore.Seeders;
using OpenDDD.Infrastructure.Persistence.EfCore.UoW;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.InMemory;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
using OpenDDD.Infrastructure.Persistence.OpenDdd.Seeders.InMemory;
using OpenDDD.Infrastructure.Persistence.OpenDdd.Seeders.Postgres;
using OpenDDD.Infrastructure.Persistence.OpenDdd.Serializers;
using OpenDDD.Infrastructure.Persistence.OpenDdd.UoW.InMemory;
using OpenDDD.Infrastructure.Persistence.OpenDdd.UoW.Postgres;
using OpenDDD.Infrastructure.Persistence.Serializers;
using OpenDDD.Infrastructure.Persistence.Storage;
using OpenDDD.Infrastructure.Persistence.Storage.InMemory;
using OpenDDD.Infrastructure.Persistence.UoW;
using OpenDDD.Infrastructure.Repository.EfCore;
using OpenDDD.Infrastructure.Repository.OpenDdd.InMemory;
using OpenDDD.Infrastructure.Repository.OpenDdd.Postgres;
using OpenDDD.Infrastructure.Service;
using OpenDDD.Infrastructure.TransactionalOutbox;
using OpenDDD.Infrastructure.TransactionalOutbox.EfCore;
using OpenDDD.Infrastructure.TransactionalOutbox.OpenDdd.InMemory;
using OpenDDD.Infrastructure.TransactionalOutbox.OpenDdd.Postgres;
using OpenDDD.Infrastructure.Utils;

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
            var options = new OpenDddOptions();
            configuration.GetSection("OpenDDD").Bind(options);
            configureOptions?.Invoke(options);

            // Prevent using EF Core generic DbContext with OpenDDD persistence
            if (options.PersistenceProvider.Equals("openddd", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Cannot use 'AddOpenDDD<TDbContext>' with OpenDDD persistence. " +
                    "Use 'AddOpenDDD' instead without specifying a DbContext.");
            }

            services.AddDbContext<TDbContext>((serviceProvider, optionsBuilder) =>
            {
                var resolvedOptions = serviceProvider.GetRequiredService<IOptions<OpenDddOptions>>().Value;

                switch (resolvedOptions.DatabaseProvider.ToLower())
                {
                    case "postgres":
                        optionsBuilder.UseNpgsql(resolvedOptions.Postgres.ConnectionString);
                        break;
                    case "sqlserver":
                        optionsBuilder.UseSqlServer(resolvedOptions.SqlServer.ConnectionString);
                        break;
                    case "sqlite":
                        optionsBuilder.UseSqlite(resolvedOptions.Sqlite.ConnectionString);
                        break;
                    case "inmemory":
                        optionsBuilder.UseInMemoryDatabase("OpenDDD");
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported Database: {resolvedOptions.DatabaseProvider}");
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
                    services.AddEfCore(options);
                    break;
                case "openddd":
                    services.AddOpenDddPersistence(options);
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
                case "rabbitmq":
                    services.AddRabbitMq();
                    break;
                case "kafka":
                    services.AddKafka();
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

        private static void AddEfCore(this IServiceCollection services, OpenDddOptions options)
        {
            services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();
            services.AddTransient<IOutboxRepository, EfCoreOutboxRepository>();
            services.AddScoped<EfCoreDatabaseSession>();
            services.AddScoped<IDatabaseSession>(provider => provider.GetRequiredService<EfCoreDatabaseSession>());
            
            if (options.AutoRegister.Repositories) RegisterEfCoreRepositories(services);
            if (options.AutoRegister.Seeders) RegisterEfCoreSeeders(services);
        }
        
        private static void AddOpenDddPersistence(this IServiceCollection services, OpenDddOptions options)
        {
            switch (options.DatabaseProvider.ToLower())
            {
                case "postgres":
                    services.AddPostgresOpenDddPersistence(options);
                    break;
                case "inmemory":
                    services.AddInMemoryOpenDddPersistence(options);
                    break;
                default:
                    throw new Exception($"Unsupported Database Provider: {options.DatabaseProvider}");
            }
            
            services.AddScoped<ISerializer, OpenDddSerializer>();
            services.AddScoped<IAggregateSerializer, OpenDddAggregateSerializer>();
        }
        
        private static void AddPostgresOpenDddPersistence(this IServiceCollection services, OpenDddOptions options)
        {
            services.AddScoped<PostgresDatabaseSession>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<OpenDddOptions>>().Value;
                var connectionString = options.Postgres.ConnectionString;
                var connection = new NpgsqlConnection(connectionString);
                return new PostgresDatabaseSession(connection);
            });
            services.AddScoped<IDatabaseSession>(provider => provider.GetRequiredService<PostgresDatabaseSession>());

            services.AddScoped<IUnitOfWork, PostgresOpenDddUnitOfWork>();
            services.AddScoped<IOutboxRepository, PostgresOpenDddOutboxRepository>();

            if (options.AutoRegister.Repositories) RegisterOpenDddPostgresRepositories(services);
            if (options.AutoRegister.Seeders) RegisterPostgresOpenDddSeeders(services);
        }
        
        private static void AddInMemoryOpenDddPersistence(this IServiceCollection services, OpenDddOptions options)
        {
            services.AddSingleton<InMemoryKeyValueStorage>();
            services.AddScoped<IKeyValueStorage>(provider => provider.GetRequiredService<InMemoryKeyValueStorage>());
            
            services.AddScoped<InMemoryDatabaseSession>();
            services.AddScoped<IDatabaseSession>(provider => provider.GetRequiredService<InMemoryDatabaseSession>());
            
            services.AddScoped<IUnitOfWork, InMemoryOpenDddUnitOfWork>();
            services.AddScoped<IOutboxRepository, InMemoryOpenDddOutboxRepository>();
            
            if (options.AutoRegister.Repositories) RegisterOpenDddInMemoryRepositories(services);
            if (options.AutoRegister.Seeders) RegisterInMemoryOpenDddSeeders(services);
        }

        private static void AddAzureServiceBus(this IServiceCollection services)
        {
            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<OpenDddOptions>>().Value;
                var azureOptions = options.AzureServiceBus ?? throw new InvalidOperationException("Azure Service Bus options are missing.");

                if (string.IsNullOrWhiteSpace(azureOptions.ConnectionString))
                {
                    throw new InvalidOperationException("Azure Service Bus connection string is missing.");
                }

                return new ServiceBusClient(azureOptions.ConnectionString);
            });

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<OpenDddOptions>>().Value;
                var azureOptions = options.AzureServiceBus ?? throw new InvalidOperationException("Azure Service Bus options are missing.");

                return new ServiceBusAdministrationClient(azureOptions.ConnectionString);
            });

            services.AddSingleton<IMessagingProvider>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<OpenDddOptions>>().Value;
                var azureOptions = options.AzureServiceBus ?? throw new InvalidOperationException("Azure Service Bus options are missing.");

                return new AzureServiceBusMessagingProvider(
                    provider.GetRequiredService<ServiceBusClient>(),
                    provider.GetRequiredService<ServiceBusAdministrationClient>(),
                    azureOptions.AutoCreateTopics,
                    provider.GetRequiredService<ILogger<AzureServiceBusMessagingProvider>>()
                );
            });
        }
        
        private static void AddRabbitMq(this IServiceCollection services)
        {
            services.AddSingleton<IMessagingProvider, RabbitMqMessagingProvider>();
        }
        
        private static void AddKafka(this IServiceCollection services)
        {
            services.AddSingleton<IMessagingProvider>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<OpenDddOptions>>().Value;
                var kafkaOptions = options.Kafka ?? throw new InvalidOperationException("Kafka options are missing.");
            
                if (string.IsNullOrWhiteSpace(kafkaOptions.BootstrapServers))
                    throw new InvalidOperationException("Kafka bootstrap servers must be configured.");

                var logger = provider.GetRequiredService<ILogger<KafkaMessagingProvider>>();
                return new KafkaMessagingProvider(
                    kafkaOptions.BootstrapServers,
                    new AdminClientBuilder(new AdminClientConfig { BootstrapServers = kafkaOptions.BootstrapServers, ClientId = "OpenDDD" }).Build(),
                    new ProducerBuilder<Null, string>(new ProducerConfig { BootstrapServers = kafkaOptions.BootstrapServers, ClientId = "OpenDDD" }).Build(),
                    new KafkaConsumerFactory(kafkaOptions.BootstrapServers),
                    kafkaOptions.AutoCreateTopics,
                    logger
                );
            });
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
            var serviceInterfaces = TypeScanner.GetRelevantTypes()
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
            RegisterRepositories(services, typeof(EfCoreRepository<,>));
        }

        private static void RegisterOpenDddPostgresRepositories(IServiceCollection services)
        {
            RegisterRepositories(services, typeof(PostgresOpenDddRepository<,>));
        }

        private static void RegisterOpenDddInMemoryRepositories(IServiceCollection services)
        {
            RegisterRepositories(services, typeof(InMemoryOpenDddRepository<,>));
        }
        
        private static void RegisterRepositories(IServiceCollection services, Type baseRepositoryType)
        {
            var aggregateTypes = TypeScanner.FindTypesDerivedFromGeneric(typeof(AggregateRootBase<>));
            var relevantTypes = TypeScanner.GetRelevantTypes().ToList();

            foreach (var aggregateType in aggregateTypes)
            {
                var idType = aggregateType.BaseType!.GetGenericArguments()[0];
                var repositoryInterfaceType = typeof(IRepository<,>).MakeGenericType(aggregateType, idType);

                var customRepositoryInterface = relevantTypes
                    .FirstOrDefault(t => t.IsInterface && repositoryInterfaceType.IsAssignableFrom(t));

                var customRepositoryImplementation = customRepositoryInterface != null
                    ? relevantTypes
                        .FirstOrDefault(t => !t.IsInterface &&
                                                  !t.IsAbstract &&
                                                  customRepositoryInterface.IsAssignableFrom(t) &&
                                                  t.BaseType?.Name.Contains(baseRepositoryType.Name) == true)
                    : null;

                if (customRepositoryInterface != null && customRepositoryImplementation != null)
                {
                    try
                    {
                        services.AddTransient(customRepositoryInterface, customRepositoryImplementation);
                        Console.WriteLine($"Registered custom repository: {TypeScanner.GetReadableTypeName(customRepositoryInterface)} → {TypeScanner.GetReadableTypeName(customRepositoryImplementation)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to register customer repository: {TypeScanner.GetReadableTypeName(customRepositoryInterface)}: {ex.Message}");
                    }
                }
                else
                {
                    var defaultRepositoryImplementationType = baseRepositoryType.MakeGenericType(aggregateType, idType);
                    services.AddTransient(repositoryInterfaceType, defaultRepositoryImplementationType);
                    Console.WriteLine($"Registered default repository: {TypeScanner.GetReadableTypeName(repositoryInterfaceType)} → {TypeScanner.GetReadableTypeName(defaultRepositoryImplementationType)}");
                }
            }
        }

        private static void RegisterActions(IServiceCollection services)
        {
            var actionTypes = TypeScanner.GetRelevantTypes(onlyConcreteClasses: true)
                .Where(type =>
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
            var listenerTypes = TypeScanner.GetRelevantTypes(onlyConcreteClasses: true)
                .Where(type =>
                    TypeScanner.IsDerivedFromGenericType(type, typeof(EventListenerBase<,>)))
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

        private static void RegisterEfCoreSeeders(IServiceCollection services)
        {
            var seederTypes = TypeScanner.GetRelevantTypes(onlyConcreteClasses: true)
                .Where(type => typeof(IEfCoreSeeder).IsAssignableFrom(type))
                .ToList();

            foreach (var seederType in seederTypes)
            {
                services.AddTransient(typeof(IEfCoreSeeder), seederType);
                Console.WriteLine($"Registered EF Core seeder: {seederType.Name} with lifetime: Transient");
            }
        }
        
        private static void RegisterPostgresOpenDddSeeders(IServiceCollection services)
        {
            var seederTypes = TypeScanner.GetRelevantTypes(onlyConcreteClasses: true)
                .Where(type => typeof(IPostgresOpenDddSeeder).IsAssignableFrom(type))
                .ToList();

            foreach (var seederType in seederTypes)
            {
                services.AddTransient(typeof(IPostgresOpenDddSeeder), seederType);
                Console.WriteLine($"Registered Postgres OpenDdd seeder: {seederType.Name} with lifetime: Transient");
            }
        }
        
        private static void RegisterInMemoryOpenDddSeeders(IServiceCollection services)
        {
            var seederTypes = TypeScanner.GetRelevantTypes(onlyConcreteClasses: true)
                .Where(type => typeof(IInMemoryOpenDddSeeder).IsAssignableFrom(type))
                .ToList();

            foreach (var seederType in seederTypes)
            {
                services.AddTransient(typeof(IInMemoryOpenDddSeeder), seederType);
                Console.WriteLine($"Registered InMemory OpenDDD seeder: {seederType.Name} with lifetime: Transient");
            }
        }
    }
}
