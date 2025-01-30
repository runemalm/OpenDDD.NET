using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.Persistence.EfCore.Base
{
    public abstract class EfEntityConfigurationBase<TEntity, TId> : IEntityTypeConfiguration<TEntity>
        where TEntity : class, IEntity<TId>
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            // Primary Key
            builder.HasKey("Id");

            // ID Property
            builder.Property(typeof(TId), "Id").IsRequired();

            // Timestamp Properties
            builder.Property<DateTime>("CreatedAt").IsRequired();
            builder.Property<DateTime>("UpdatedAt").IsRequired();
            
            // Table Name
            builder.ToTable(GetTableName());
        }

        protected string GetTableName()
        {
            return ToPlural(typeof(TEntity).Name);
        }
        
        private string ToPlural(string name)
        {
            if (name.EndsWith("y"))
                return name.Substring(0, name.Length - 1) + "ies";
            if (name.EndsWith("s"))
                return name;
            return name + "s";
        }
    }
}
