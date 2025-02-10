using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
using OpenDDD.Infrastructure.Repository.OpenDdd.Postgres;
using OpenDDD.Infrastructure.Persistence.Serializers;
using Npgsql;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Repositories.OpenDdd.Postgres
{
    public class PostgresOpenDddCustomerRepository : PostgresOpenDddRepository<Customer, Guid>, ICustomerRepository
    {
        private readonly ILogger<PostgresOpenDddCustomerRepository> _logger;

        public PostgresOpenDddCustomerRepository(
            PostgresDatabaseSession session, 
            IAggregateSerializer serializer, 
            ILogger<PostgresOpenDddCustomerRepository> logger) 
            : base(session, serializer)
        {
            _logger = logger;
        }

        public async Task<Customer> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
            }

            try
            {
                const string query = "SELECT data FROM customers WHERE data->>'email' = @email LIMIT 1;";
                await using var cmd = new NpgsqlCommand(query, Session.Connection, Session.Transaction);
                cmd.Parameters.AddWithValue("email", email);

                var result = await cmd.ExecuteScalarAsync(ct);

                if (result is string json)
                {
                    return Serializer.Deserialize<Customer, Guid>(json) 
                        ?? throw new KeyNotFoundException($"No customer found with email '{email}'.");
                }

                throw new KeyNotFoundException($"No customer found with email '{email}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving customer by email: {Email}", email);
                throw;
            }
        }

        public async Task<Customer?> FindByEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
            }

            const string query = "SELECT data FROM customers WHERE data->>'email' = @email LIMIT 1;";
            await using var cmd = new NpgsqlCommand(query, Session.Connection, Session.Transaction);
            cmd.Parameters.AddWithValue("email", email);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result is string json ? Serializer.Deserialize<Customer, Guid>(json) : null;
        }
    }
}
