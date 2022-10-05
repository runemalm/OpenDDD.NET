using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using DDD.Application.Exceptions;
using Npgsql;

namespace DDD.Infrastructure.Services.Persistence.Postgres
{
    public class PostgresConnection : Connection
    {
        private NpgsqlConnection _sqlConn;
        private NpgsqlTransaction _sqlTrx;
        
        public PostgresConnection(string connString) : base(connString)
        {
            
        }

        public override async Task Open()
        {
            _sqlConn = new NpgsqlConnection(_connString);
            await _sqlConn.OpenAsync();
        }
        
        public override async Task Close()
        {
            await _sqlConn.CloseAsync();
        }

        public override Task StartTransactionAsync()
        {
            if (_sqlConn == null)
                throw new DddException(
                    "Can't start transaction, no connection has been made.");
            else if (_sqlConn.State != ConnectionState.Open)
                throw new DddException(
                    "Can't start transaction, the connection is not in open state.");
            _sqlTrx = _sqlConn.BeginTransaction();
            return Task.CompletedTask;
        }
        
        public override async Task CommitTransactionAsync()
        {
            if (_sqlTrx == null)
                throw new DddException(
                    "Can't commit non-existing transaction.");
            await _sqlTrx.CommitAsync();
            _sqlTrx.Dispose();
            _sqlTrx = null;
        }
        
        public override async Task RollbackTransactionAsync()
        {
            if (_sqlTrx == null)
                throw new DddException(
                    "Can't rollback non-existing transaction.");
            await _sqlTrx.RollbackAsync();
            _sqlTrx.Dispose();
            _sqlTrx = null;
        }
        
        public override async Task<int> ExecuteNonQueryAsync(string stmt, IDictionary<string, object> parameters)
        {
            var cmd = new NpgsqlCommand(stmt, _sqlConn, _sqlTrx);
			
            if (parameters != null)
                foreach (KeyValuePair<string, object> kvp in parameters)
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
			
            return await cmd.ExecuteNonQueryAsync();
        }

        public override async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt, IDictionary<string, object> parameters)
        {
            var aggregates = new List<T>();
            
            var cmd = new NpgsqlCommand(stmt, _sqlConn, _sqlTrx);
            
            if (parameters != null)
                foreach (KeyValuePair<string, object> kvp in parameters)
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
            
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var aggregate = JsonSerializer.Deserialize<T>(reader.GetFieldValue<string>(1));
                    aggregates.Add(aggregate);
                }
            }

            return aggregates;
        }
        
        public override async Task<T> ExecuteScalarAsync<T>(string stmt, IDictionary<string, object> parameters)
        {
            var cmd = new NpgsqlCommand(stmt, _sqlConn, _sqlTrx);

            if (parameters != null)
                foreach (KeyValuePair<string, object> kvp in parameters)
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);

            T scalar = (T)await cmd.ExecuteScalarAsync();

            return scalar;
        }
    }
}
