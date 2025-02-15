using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using Bookstore.Domain.Model;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Infrastructure.Persistence.EfCore.Configurations
{
    public class OrderConfiguration : EfAggregateRootConfigurationBase<Order, Guid>
    {
        public override void Configure(EntityTypeBuilder<Order> builder)
        {
            base.Configure(builder);
            
            builder
                .HasMany(o => o.LineItems)
                .WithOne()
                .HasForeignKey("OrderId") // Shadow property for FK
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Navigation(o => o.LineItems).AutoInclude();
        }
    }
}
