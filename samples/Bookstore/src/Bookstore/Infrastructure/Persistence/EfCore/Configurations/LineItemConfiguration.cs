using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Persistence.EfCore.Configurations
{
    public class LineItemConfiguration : EfEntityConfigurationBase<LineItem, Guid>
    {
        public override void Configure(EntityTypeBuilder<LineItem> builder)
        {
            base.Configure(builder);
            
            // Custom configurations here
            builder.Property<Guid>("OrderId")
                .IsRequired();
            
            builder.OwnsOne(li => li.Price, money =>
            {
                money.Property(m => m.Amount).HasColumnName("Price_Amount");
                money.Property(m => m.Currency).HasColumnName("Price_Currency").HasMaxLength(3);
            });
        }
    }
}
