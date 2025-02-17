using Bookstore.Domain.Model;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.InMemory;
using OpenDDD.Infrastructure.Persistence.OpenDdd.Seeders.InMemory;
using OpenDDD.Infrastructure.Persistence.Serializers;

namespace Bookstore.Infrastructure.Persistence.OpenDdd.Seeders.InMemory
{
    public class InMemoryOpenDddBooksSeeder : IInMemoryOpenDddSeeder
    {
        private readonly IAggregateSerializer _serializer;

        public InMemoryOpenDddBooksSeeder(IAggregateSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public async Task ExecuteAsync(InMemoryDatabaseSession session, CancellationToken ct)
        {
            const string tableName = "books";

            var existingBooks = await session.SelectAllAsync<string>(tableName, ct);
            if (existingBooks.Any()) return;

            var blueBook = Book.Create("Domain-Driven Design: Tackling Complexity in the Heart of Software", "Eric Evans", 2003, Money.USD(48.71m));
            var redBook = Book.Create("Implementing Domain-Driven Design", "Vaughn Vernon", 2013, Money.USD(45.84m));

            await session.UpsertAsync(tableName, blueBook.Id, _serializer.Serialize<Book, Guid>(blueBook), ct);
            await session.UpsertAsync(tableName, redBook.Id, _serializer.Serialize<Book, Guid>(redBook), ct);
        }
    }
}
