using Microsoft.EntityFrameworkCore;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using Bookstore.Domain.Model;
using OpenDDD.API.Options;

namespace Bookstore.Infrastructure.Persistence.EfCore
{
    public class BookstoreDbContext : OpenDddDbContextBase
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }

        public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options, OpenDddOptions openDddOptions)
            : base(options, openDddOptions)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Custom configurations here
        }
    }
}
