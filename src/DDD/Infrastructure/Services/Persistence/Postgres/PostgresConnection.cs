using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Npgsql;
using DDD.Domain.Model.Error;

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

        public override void Open()
        {
            _sqlConn = new NpgsqlConnection(_connString);
            _sqlConn.Open();
        }
        
        public override async Task OpenAsync()
        {
            _sqlConn = new NpgsqlConnection(_connString);
            await _sqlConn.OpenAsync();
        }
        
        public override void Close()
        {
            _sqlConn.Close();
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
            if (_sqlConn.State != ConnectionState.Open)
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
            await _sqlTrx.DisposeAsync();
            _sqlTrx = null;
        }
        
        public override async Task RollbackTransactionAsync()
        {
            if (_sqlTrx == null)
                throw new DomainException(
                    "Can't rollback non-existing transaction.");
            await _sqlTrx.RollbackAsync();
            await _sqlTrx.DisposeAsync();
            _sqlTrx = null;
        }
        
        public override int ExecuteNonQuery(string stmt, IDictionary<string, object> parameters)
        {
            var cmd = BuildCmdWithParams(stmt, parameters);
            return cmd.ExecuteNonQuery();
        }
        
        public override async Task<int> ExecuteNonQueryAsync(string stmt, IDictionary<string, object> parameters)
        {
            var cmd = BuildCmdWithParams(stmt, parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        private NpgsqlCommand BuildCmdWithParams(string stmt, IDictionary<string, object> parameters)
        {
            var cmd = new NpgsqlCommand(stmt, _sqlConn, _sqlTrx);
            if (parameters != null)
                foreach (KeyValuePair<string, object> kvp in parameters)
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
            return cmd;
        }

        public override IEnumerable<T> ExecuteQuery<T>(string stmt, IDictionary<string, object> parameters)
        {
            var aggregates = new List<T>();
            
            var cmd = BuildCmdWithParams(stmt, parameters);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var aggregate = JsonConvert.DeserializeObject<T>(reader.GetFieldValue<string>(1), _serializerSettings);
                aggregates.Add(aggregate);
            }

            return aggregates;
        }
        
        public override async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt, IDictionary<string, object> parameters)
        {
            var aggregates = new List<T>();
            
            var cmd = BuildCmdWithParams(stmt, parameters);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var aggregate = JsonConvert.DeserializeObject<T>(reader.GetFieldValue<string>(1), _serializerSettings);
                aggregates.Add(aggregate);
            }

            return aggregates;
        }

        public override T ExecuteScalar<T>(string stmt, IDictionary<string, object> parameters)
        {
            var cmd = BuildCmdWithParams(stmt, parameters);
            var result = cmd.ExecuteScalar();
            return result != null ? (T)result : default(T);
        }

        public override async Task<T> ExecuteScalarAsync<T>(string stmt, IDictionary<string, object>? parameters)
        {
            var cmd = BuildCmdWithParams(stmt, parameters);
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? (T)result : default(T);
        }
    }
}
