using System.Linq.Expressions;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Infrastructure.Persistence.Serializers;
using OpenDDD.Infrastructure.Repository.OpenDdd.Base;
using OpenDDD.API.Extensions;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
using Npgsql;
using OpenDDD.Infrastructure.Persistence.OpenDdd.Expressions;

namespace OpenDDD.Infrastructure.Repository.OpenDdd.Postgres
{
    public class PostgresOpenDddRepository<TAggregateRoot, TId> : OpenDddRepositoryBase<TAggregateRoot, TId, PostgresDatabaseSession>
        where TAggregateRoot : AggregateRootBase<TId>
        where TId : notnull
    {
        protected readonly PostgresDatabaseSession Session;
        private readonly string _tableName;

        public PostgresOpenDddRepository(PostgresDatabaseSession session, IAggregateSerializer serializer) 
            : base(session, serializer)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            _tableName = typeof(TAggregateRoot).Name.ToLower().Pluralize();
        }
        
        public override async Task<TAggregateRoot> GetAsync(TId id, CancellationToken ct)
        {
            var entity = await FindAsync(id, ct);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID '{id}' was not found in {_tableName}.");
            }
            return entity;
        }

        public override async Task<TAggregateRoot?> FindAsync(TId id, CancellationToken ct)
        {
            var query = $"SELECT data FROM {_tableName} WHERE id = @id;";
            await using var cmd = new NpgsqlCommand(query, Session.Connection, Session.Transaction);
            cmd.Parameters.AddWithValue("id", id!);

            var result = await cmd.ExecuteScalarAsync(ct);
            return result != null ? Serializer.Deserialize<TAggregateRoot, TId>(result.ToString()!) : null;
        }
        
        public override async Task<IEnumerable<TAggregateRoot>> FindWithAsync(
            Expression<Func<TAggregateRoot, bool>> filterExpression, CancellationToken ct)
        {
            if (filterExpression == null) throw new ArgumentNullException(nameof(filterExpression));

            var whereClause = JsonbExpressionParser.Parse(filterExpression);
            var query = $"SELECT data FROM {_tableName} WHERE {whereClause};";

            await using var cmd = new NpgsqlCommand(query, Session.Connection, Session.Transaction);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var results = new List<TAggregateRoot>();

            while (await reader.ReadAsync(ct))
            {
                var serializedData = reader.GetString(0);
                results.Add(Serializer.Deserialize<TAggregateRoot, TId>(serializedData));
            }

            return results;
        }

        public override async Task<IEnumerable<TAggregateRoot>> FindAllAsync(CancellationToken ct)
        {
            var query = $"SELECT data FROM {_tableName};";
            await using var cmd = new NpgsqlCommand(query, Session.Connection, Session.Transaction);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var results = new List<TAggregateRoot>();
            while (await reader.ReadAsync(ct))
            {
                var serializedData = reader.GetString(0);
                results.Add(Serializer.Deserialize<TAggregateRoot, TId>(serializedData));
            }
            return results;
        }

        public override async Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            var query = $@"
                INSERT INTO {_tableName} (id, data) 
                VALUES (@id, @data) 
                ON CONFLICT (id) 
                DO UPDATE SET data = EXCLUDED.data;";

            aggregateRoot.UpdatedAt = DateTime.UtcNow;
            var serializedData = Serializer.Serialize<TAggregateRoot, TId>(aggregateRoot);
            await using var cmd = new NpgsqlCommand(query, Session.Connection, Session.Transaction);
            cmd.Parameters.AddWithValue("id", aggregateRoot.Id!);
            cmd.Parameters.Add("data", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = serializedData;

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public override async Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            var query = $"DELETE FROM {_tableName} WHERE id = @id;";
            await using var cmd = new NpgsqlCommand(query, Session.Connection, Session.Transaction);
            cmd.Parameters.AddWithValue("id", aggregateRoot.Id!);

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
