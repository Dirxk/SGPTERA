using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace TeracromDatabase
{

    public class DapperContext : IDisposable
    {
        private readonly IConfiguration _configuration;
        private IDbConnection _dbConnection;
        private IDbTransaction _transaccion;
        private bool _confirmCommit = false;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;

            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        }

        public IDbConnection AbrirConexion(string cadenaConexion, bool useTransaction = false)
        {
            try
            {
                _dbConnection = new SqlConnection(_configuration.GetConnectionString(cadenaConexion));

                if (_dbConnection.State != ConnectionState.Open)
                    _dbConnection.Open();

                if (useTransaction)
                {
                    _transaccion = _dbConnection.BeginTransaction();
                }
            }
            catch (SqlException ex) { throw ex; }

            return _dbConnection;
        }

        public void IniciarTransaccion()
        {
            _transaccion = _dbConnection.BeginTransaction();
        }
        public void ConfirmarTransaccion()
        {
            if (_transaccion != null)
            {
                _confirmCommit = true;
                _transaccion.Commit();
            }
        }
        public void EliminarTransaccion()
        {
            if (_transaccion != null)
                _transaccion.Rollback();
        }
        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            return await _dbConnection.QueryAsync<T>(sql, parameters, _transaccion);
        }
        public async Task<IEnumerable<T>> QueryStoredProcedureAsync<T>(string sql, object? parameters = null)
        {
            return await _dbConnection.QueryAsync<T>(sql, parameters, _transaccion, commandType: CommandType.StoredProcedure);
        }
        public async Task<T> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<T>(sql, parameters, _transaccion);
        }
        public async Task<List<IEnumerable<object>>> QueryMultipleAsync(string sql, object? parameters = null, int resultSets = 1)
        {
            var resultList = new List<IEnumerable<object>>();

            using var multi = await _dbConnection.QueryMultipleAsync(sql, parameters, _transaccion);

            for (int i = 0; i < resultSets; i++)
            {
                if (!multi.IsConsumed)
                {
                    var result = await multi.ReadAsync<object>();
                    resultList.Add(result);
                }
            }

            return resultList;
        }
        public async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            return await _dbConnection.ExecuteAsync(sql, parameters, _transaccion);
        }
        public async Task<object> ExecuteScalarAsync(string sql, object? parameters = null)
        {
            return await _dbConnection.ExecuteScalarAsync<object>(sql, parameters, _transaccion);
        }
        public void Dispose()
        {
            if (_transaccion != null && !_confirmCommit)
            {
                _transaccion?.Rollback();
            }

            _transaccion?.Dispose();
            _dbConnection?.Close();
            _dbConnection?.Dispose();
        }
    }
}
