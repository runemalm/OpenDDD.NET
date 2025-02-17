using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Persistence.EfCore.Configurations
{
    public class BookConfiguration : EfAggregateRootConfigurationBase<Book, Guid>
    {
        public override void Configure(EntityTypeBuilder<Book> builder)
        {
            base.Configure(builder);
            
            // Custom configurations here
            builder.OwnsOne(li => li.Price, money =>
            {
                money.Property(m => m.Amount).HasColumnName("Price_Amount");
                money.Property(m => m.Currency).HasColumnName("Price_Currency").HasMaxLength(3);
            });
        }
    }
}
