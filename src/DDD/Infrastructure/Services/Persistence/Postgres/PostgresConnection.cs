using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Npgsql;
using DDD.Domain.Model.Error;
using PostgresException = DDD.Infrastructure.Ports.Adapters.Common.Exceptions.PostgresException;

namespace DDD.Infrastructure.Services.Persistence.Postgres
{
    public class PostgresConnection : Connection
    {
        private NpgsqlConnection _sqlConn;
        private NpgsqlTransaction _sqlTrx;

        public PostgresConnection(string connString, JsonSerializerSettings serializerSettings) 
            : base(connString, serializerSettings)
        {
            
        }

        public override async Task OpenAsync()
        {
            _sqlConn = new NpgsqlConnection(_connString);
            await _sqlConn.OpenAsync();
        }
        
        public override async Task CloseAsync()
        {
            await _sqlConn.CloseAsync();
        }

        public override Task StartTransactionAsync()
        {
            if (_sqlConn == null)
                throw new DomainException(
                    "Can't start transaction, no connection has been made.");
            else if (_sqlConn.State != ConnectionState.Open)
                throw new DomainException(
                    "Can't start transaction, the connection is not in an open state.");
            _sqlTrx = _sqlConn.BeginTransaction();
            return Task.CompletedTask;
        }
        
        public override async Task CommitTransactionAsync()
        {
            if (_sqlTrx == null)
                throw new DomainException(
                    "Can't commit non-existing transaction.");
            await _sqlTrx.CommitAsync();
            _sqlTrx.Dispose();
            _sqlTrx = null;
        }
        
        public override async Task RollbackTransactionAsync()
        {
            if (_sqlTrx == null)
                throw new DomainException(
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
                    var aggregate = JsonConvert.DeserializeObject<T>(reader.GetFieldValue<string>(1), _serializerSettings);
                    aggregates.Add(aggregate);
                }
            }

            return aggregates;
        }
        
        public override async Task<T> ExecuteScalarAsync<T>(string stmt, IDictionary<string, object>? parameters)
        {
            var cmd = new NpgsqlCommand(stmt, _sqlConn, _sqlTrx);

            if (parameters != null)
                foreach (KeyValuePair<string, object> kvp in parameters)
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);

            var result = await cmd.ExecuteScalarAsync();

            return result != null ? (T)result : default(T);
        }
    }
}
