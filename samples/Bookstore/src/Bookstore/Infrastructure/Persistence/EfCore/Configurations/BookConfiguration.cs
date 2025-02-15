using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        }
    }
}
