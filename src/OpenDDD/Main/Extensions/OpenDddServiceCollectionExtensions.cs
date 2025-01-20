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
                
                if (options.AutoRegisterActions)
                {
                    RegisterActions(services);
                }

                if (options.AutoRegisterRepositories)
                {
                    RegisterRepositories(services);
                }
            });

            // Allow additional service configuration
            configureServices?.Invoke(services);

            // Register the service manager and startup filter
            RegisterServiceManager(services);
            services.AddSingleton<IStartupFilter, OpenDddStartupFilter>();

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

        private static void RegisterRepositories(IServiceCollection services)
        {
            var repositoryTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.Name.EndsWith("Repository") &&
                               typeof(IRepository<,>).IsAssignableTo(type) &&
                               !type.IsInterface && !type.IsAbstract);

            foreach (var repositoryType in repositoryTypes)
            {
                services.AddTransient(repositoryType);
                Console.WriteLine($"Registered repository: {repositoryType.Name}");
            }
        }
        
        private static void RegisterServiceManager(IServiceCollection services)
        {
            services.AddSingleton<IOpenDddServiceManager, OpenDddServiceManager>();
        }
    }
}
