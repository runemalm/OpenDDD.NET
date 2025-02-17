using Microsoft.EntityFrameworkCore;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.API.Options;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Persistence.EfCore
{
    public class BookstoreDbContext : OpenDddDbContextBase
    {
        public DbSet<Book> Books { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

        public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options, OpenDddOptions openDddOptions, ILogger<BookstoreDbContext> logger)
            : base(options, openDddOptions, logger)
        {
            
        }
    }
}
