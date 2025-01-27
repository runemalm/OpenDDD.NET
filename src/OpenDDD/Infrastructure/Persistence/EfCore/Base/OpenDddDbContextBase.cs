using System.Reflection;
using Microsoft.EntityFrameworkCore;
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
        }

        private void ApplyConfigurations(ModelBuilder modelBuilder)
        {
            var configurationTypes = Assembly.GetEntryAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract &&
                                 t.BaseType != null &&
                                 (t.BaseType.IsGenericType &&
                                    (
                                        t.BaseType.GetGenericTypeDefinition() == typeof(EfAggregateRootConfigurationBase<,>) ||
                                        t.BaseType.GetGenericTypeDefinition() == typeof(EfEntityConfigurationBase<,>)
                                    )
                                 ));

            foreach (var configType in configurationTypes)
            {
                var configurationInstance = Activator.CreateInstance(configType);
                modelBuilder.ApplyConfiguration((dynamic)configurationInstance!);
            }
        }
    }
}
