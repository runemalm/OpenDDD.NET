using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.Persistence.EfCore.Base
{
    public class OpenDddDbContextBase : DbContext
    {
        private readonly OpenDddOptions _openDddOptions;

        public OpenDddDbContextBase(DbContextOptions options, OpenDddOptions openDddOptions) : base(options)
        {
            _openDddOptions = openDddOptions;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            if (_openDddOptions.AutoRegisterConfigurations)
            {
                ApplyConfigurations(modelBuilder);
            }

            ValidateModelConfiguration(modelBuilder);
        }

        public void ApplyConfigurations(ModelBuilder modelBuilder)
        {
            var configurationTypes = Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract &&
                            t.BaseType != null &&
                            (t.BaseType.IsGenericType &&
                             (t.BaseType.GetGenericTypeDefinition() == typeof(EfAggregateRootConfigurationBase<,>) ||
                              t.BaseType.GetGenericTypeDefinition() == typeof(EfEntityConfigurationBase<,>))));

            foreach (var configType in configurationTypes)
            {
                var configurationInstance = Activator.CreateInstance(configType);
                modelBuilder.ApplyConfiguration((dynamic)configurationInstance!);
            }
        }

        public void ValidateModelConfiguration(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
            var configuredEntities = modelBuilder.Model.GetEntityTypes().Select(e => e.ClrType).ToList();

            var allEntities = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => !t.IsAbstract &&
                                 (t.BaseType != null &&
                                 (t.BaseType.IsGenericType &&
                                 (t.BaseType.GetGenericTypeDefinition() == typeof(AggregateRootBase<>) ||
                                 t.BaseType.GetGenericTypeDefinition() == typeof(EntityBase<>)))))
                .ToList();

            var unconfiguredEntities = allEntities.Except(configuredEntities).ToList();
            if (unconfiguredEntities.Any())
            {
                throw new InvalidOperationException(
                    $"The following entities are not explicitly configured: {string.Join(", ", unconfiguredEntities.Select(e => e.Name))}.");
            }
        }
    }
}
