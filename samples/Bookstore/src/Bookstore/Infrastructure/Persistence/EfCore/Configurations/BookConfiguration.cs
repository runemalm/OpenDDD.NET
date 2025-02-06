using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Persistence.EfCore.Configurations
{
    public class BookConfiguration : EfAggregateRootConfigurationBase<Book, Guid>
    {
        
    }
}
