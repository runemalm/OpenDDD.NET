using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenDDD.API.Options;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Infrastructure.TransactionalOutbox;

namespace OpenDDD.Infrastructure.Persistence.EfCore.Base
{
    public class OpenDddDbContextBase : DbContext
    {
        private readonly OpenDddOptions _openDddOptions;
        private readonly ILogger<OpenDddDbContextBase> _logger;
        
        public DbSet<OutboxEntry> OutboxEntries { get; set; } = null!;

        public OpenDddDbContextBase(DbContextOptions options, OpenDddOptions openDddOptions, ILogger<OpenDddDbContextBase> logger) 
            : base(options)
        {
            _openDddOptions = openDddOptions;
            _logger = logger;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure OutboxEntry Table
            modelBuilder.Entity<OutboxEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EventType).IsRequired();
                entity.Property(e => e.EventName).IsRequired();
                entity.Property(e => e.Payload).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ProcessedAt).IsRequired(false);
                entity.Property(e => e.Processed).HasDefaultValue(false);
            });

            if (_openDddOptions.AutoRegister.EfCoreConfigurations)
            {
                ApplyConfigurations(modelBuilder);
            }

            ValidateModelConfiguration(modelBuilder);
        }

        public void ApplyConfigurations(ModelBuilder modelBuilder)
        {
            var configurationTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract &&
                       t.BaseType != null &&
                       (t.BaseType.IsGenericType &&
                       (t.BaseType.GetGenericTypeDefinition() == typeof(EfAggregateRootConfigurationBase<,>) ||
                       t.BaseType.GetGenericTypeDefinition() == typeof(EfEntityConfigurationBase<,>))));

            foreach (var configType in configurationTypes)
            {
                var configurationInstance = Activator.CreateInstance(configType);
                modelBuilder.ApplyConfiguration((dynamic)configurationInstance!);
                _logger.LogInformation("Auto-loaded EF Core configuration: {ConfigurationType}", configType.Name);
            }
        }

        public void ValidateModelConfiguration(ModelBuilder modelBuilder)
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
