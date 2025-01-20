using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenDDD.Application;
using OpenDDD.Domain.Model;
using OpenDDD.Main.Managers;
using OpenDDD.Main.Options;

namespace OpenDDD.Main.Extensions
{
    public static class OpenDddServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenDDD(this IServiceCollection services, IConfiguration configuration,
            Action<OpenDddServiceOptions>? configureOptions = null)
        {
            var options = new OpenDddServiceOptions();
            configuration.GetSection("OpenDDD:Services").Bind(options);
            configureOptions?.Invoke(options);

            if (options.AutoRegisterActions)
            {
                RegisterActions(services);
            }

            if (options.AutoRegisterRepositories)
            {
                RegisterRepositories(services);
            }

            RegisterServiceManager(services);

            return services;
        }

        private static void RegisterActions(IServiceCollection services)
        {
            var actionTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IAction<,>).IsAssignableTo(type) && !type.IsInterface && !type.IsAbstract);

            foreach (var actionType in actionTypes)
            {
                services.AddTransient(actionType);
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
            }
        }
        
        private static void RegisterServiceManager(IServiceCollection services)
        {
            services.AddSingleton<IOpenDddServiceManager, OpenDddServiceManager>();
        }
    }
}
