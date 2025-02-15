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

            var existingBooks = await session.LoadAllAsync<string>(tableName, ct);
            if (existingBooks.Any()) return;

            var book1 = Book.Create("Domain-Driven Design: Tackling Complexity in the Heart of Software", "Eric Evans", 2003);
            var book2 = Book.Create("Implementing Domain-Driven Design", "Vaughn Vernon", 2013);

            await session.SaveAsync(tableName, book1.Id, _serializer.Serialize<Book, Guid>(book1), ct);
            await session.SaveAsync(tableName, book2.Id, _serializer.Serialize<Book, Guid>(book2), ct);
        }
    }
}
