using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
using OpenDDD.Infrastructure.Persistence.Serializers;
using OpenDDD.Infrastructure.Persistence.OpenDdd.Seeders.Postgres;
using Npgsql;
using NpgsqlTypes;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Persistence.OpenDdd.Seeders.Postgres
{
    public class PostgresOpenDddBooksSeeder : IPostgresOpenDddSeeder
    {
        private readonly IAggregateSerializer _serializer;

        public PostgresOpenDddBooksSeeder(IAggregateSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public async Task ExecuteAsync(PostgresDatabaseSession session, CancellationToken ct)
        {
            const string checkExistsQuery = "SELECT COUNT(*) FROM books;";
            await using var checkCmd = new NpgsqlCommand(checkExistsQuery, session.Connection, session.Transaction);
            var count = (long)(await checkCmd.ExecuteScalarAsync(ct) ?? 0);

            if (count == 0)
            {
                const string insertQuery = @"
                    INSERT INTO books (id, data) VALUES
                    (@id1, @data1::jsonb),
                    (@id2, @data2::jsonb);";

                var blueBook = Book.Create("Domain-Driven Design: Tackling Complexity in the Heart of Software", "Eric Evans", 2003, Money.USD(48.71m));
                var redBook = Book.Create("Implementing Domain-Driven Design", "Vaughn Vernon", 2013, Money.USD(45.84m));

                await using var insertCmd = new NpgsqlCommand(insertQuery, session.Connection, session.Transaction);
                insertCmd.Parameters.AddWithValue("id1", blueBook.Id);
                insertCmd.Parameters.Add("data1", NpgsqlDbType.Jsonb).Value = _serializer.Serialize<Book, Guid>(blueBook);
                insertCmd.Parameters.AddWithValue("id2", redBook.Id);
                insertCmd.Parameters.Add("data2", NpgsqlDbType.Jsonb).Value = _serializer.Serialize<Book, Guid>(redBook);

                await insertCmd.ExecuteNonQueryAsync(ct);
            }
        }
    }
}
