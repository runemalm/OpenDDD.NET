using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.Persistence.EfCore.Base
{
    public abstract class EfAggregateRootConfigurationBase<TAggregateRoot, TId> : IEntityTypeConfiguration<TAggregateRoot>
        where TAggregateRoot : class, IAggregateRoot<TId>
    {
        public virtual void Configure(EntityTypeBuilder<TAggregateRoot> builder)
        {
            // Primary Key
            builder.HasKey("Id");

            // ID Property
            builder.Property(typeof(TId), "Id").IsRequired();
            
            // Timestamp Properties
            builder.Property(typeof(DateTime), "CreatedAt").IsRequired();
            builder.Property(typeof(DateTime), "UpdatedAt").IsRequired();
            
            // Table Name
            builder.ToTable(GetTableName());
        }

        protected string GetTableName()
        {
            return ToPlural(typeof(TAggregateRoot).Name);
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
