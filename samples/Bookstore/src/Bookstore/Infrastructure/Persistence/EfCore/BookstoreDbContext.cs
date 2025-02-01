using Microsoft.EntityFrameworkCore;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.API.Options;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Persistence.EfCore
{
    public class BookstoreDbContext : OpenDddDbContextBase
    {
        public DbSet<Customer> Customers { get; set; }

        public BookstoreDbContext(DbContextOptions<BookstoreDbContext> options, OpenDddOptions openDddOptions)
            : base(options, openDddOptions)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
