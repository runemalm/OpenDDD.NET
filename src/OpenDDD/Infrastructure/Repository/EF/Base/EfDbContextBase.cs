using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.Repository.EF.Base
{
    public abstract class EfDbContextBase : DbContext
    {
        protected EfDbContextBase(DbContextOptions options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ApplyConfigurationsFromAssembly(modelBuilder, Assembly.GetExecutingAssembly());

            foreach (var assembly in EfDbContextConfiguration.AdditionalAssemblies)
            {
                ApplyConfigurationsFromAssembly(modelBuilder, assembly);
            }
        }

        protected void ApplyConfigurationsFromAssembly(ModelBuilder modelBuilder, Assembly assembly)
        {
            var configurations = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
                .ToList();

            foreach (var config in configurations)
            {
                dynamic configurationInstance = Activator.CreateInstance(config)!;
                modelBuilder.ApplyConfiguration(configurationInstance);
            }
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries<IEntity<Guid>>();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = entry.Entity.CreatedAt;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Property(e => e.CreatedAt).IsModified = false;
                }
            }
        }
    }
}
