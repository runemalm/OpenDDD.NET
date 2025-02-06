using Microsoft.EntityFrameworkCore;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;
using OpenDDD.Infrastructure.Persistence.EfCore.Seeders;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Persistence.EfCore.Seeders
{
    public class BooksSeeder : IEfCoreSeeder
    {
        public async Task ExecuteAsync(OpenDddDbContextBase dbContext, CancellationToken ct)
        {
            // Seed banks and integrations
            if (!await dbContext.Set<Book>().AnyAsync())
            {
                // Create banks
                var blueBook = Book.Create("Domain-Driven Design: Tackling Complexity in the Heart of Software", "Eric Evans", 2003);
                var redBook = Book.Create("Implementing Domain-Driven Design", "Vaughn Vernon", 2013);

                // Add banks (and their integrations through EF Core cascading)
                dbContext.Set<Book>().AddRange(blueBook, redBook);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
