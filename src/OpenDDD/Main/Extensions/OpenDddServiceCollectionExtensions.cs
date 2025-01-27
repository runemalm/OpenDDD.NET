using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenDDD.Application;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Domain.Service;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Infrastructure.Persistence.EfCore.Startup;
using OpenDDD.Infrastructure.Persistence.EfCore.UoW;
using OpenDDD.Infrastructure.Persistence.UoW;
using OpenDDD.Infrastructure.Repository.EfCore;
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
            // Configure OpenDddOptions
            var options = new OpenDddOptions();
            configuration.GetSection("OpenDDD").Bind(options);
            configureOptions?.Invoke(options);
            
            services.AddSingleton(options);

            // Validate options
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                options.ConnectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(options.ConnectionString) &&
                options.PersistenceProvider.ToLower() == "efcore" &&
                options.StorageProvider.ToLower() != "inmemory")
            {
                throw new InvalidOperationException("OpenDDD with EfCore requires a valid connection string. Please set " +
                                                    "'OpenDDD:ConnectionString' or " +
                                                    "'ConnectionStrings:DefaultConnection'.");
            }

            // Register services if persistence provider is EfCore
            if (options.PersistenceProvider.ToLower() == "efcore")
            {
                // Regisgter the DbContext using the generic type parameter
                services.AddDbContext<TDbContext>((serviceProvider, optionsBuilder) =>
                {
                    var openDddOptions = serviceProvider.GetRequiredService<OpenDddOptions>();

                    switch (options.StorageProvider.ToLower())
                    {
                        case "postgres":
                            optionsBuilder.UseNpgsql(openDddOptions.ConnectionString);
                            break;
                        case "sqlserver":
                            optionsBuilder.UseSqlServer(openDddOptions.ConnectionString);
                            break;
                        case "sqlite":
                            optionsBuilder.UseSqlite(openDddOptions.ConnectionString);
                            break;
                        case "inmemory":
                            optionsBuilder.UseInMemoryDatabase("OpenDDD");
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported StorageProvider: {openDddOptions.StorageProvider}");
                    }

                    dbContextOptions?.Invoke(optionsBuilder);
                });
            
                // Also register with the base class
                services.AddScoped<OpenDddDbContextBase>(sp => sp.GetRequiredService<TDbContext>());
                
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

            // Auto-register repositories
            if (options.AutoRegisterRepositories)
            {
                RegisterEfCoreRepositories(services);
            }

            // Auto-register domain services
            if (options.AutoRegisterDomainServices)
            {
                RegisterDomainServices(services);
            }

            // Auto-register actions
            if (options.AutoRegisterActions)
            {
                RegisterActions(services);
            }

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
