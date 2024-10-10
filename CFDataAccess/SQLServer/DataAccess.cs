using CFDataAccess.SQLServer.Utils;
using System.Data;
using System.Data.SqlClient;

namespace CFDataAccess.SQLServer
{
    public class DataAccess : IDataAccess
    {
        private readonly ConnectionManager _connectionManager;
        public DataAccess(string _connectionString)
        {
            _connectionManager = new ConnectionManager(_connectionString);
        }

        public bool BulkInsert(DataTable _dTable, string destinationTableName)
        {
            try
            {
                using (var connection = _connectionManager.GetConnection())
                {
                    connection.Open();
                    using (var bulkCopy = new SqlBulkCopy(connection))
                    {
                        bulkCopy.DestinationTableName = destinationTableName;
                        bulkCopy.WriteToServer(_dTable);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void Dispose()
        {
            _connectionManager.Dispose();
        }
        public bool ExecuteBatch(List<(string query, SqlParameter[] parameters)> queries, QueryTypeEnum queryType)
        {
            try
            {
                using (SqlConnection connection = _connectionManager.GetConnection())
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandType = (queryType == QueryTypeEnum.SelectQuery ? CommandType.Text : CommandType.StoredProcedure);
                        command.Parameters.Clear();
                        foreach (var (query, _parameters) in queries)
                        {
                            command.CommandText = query;
                            if (_parameters != null)
                            {
                                command.Parameters.AddRange(_parameters);
                            }
                            command.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public int ExecuteNonQuery(string query, QueryTypeEnum queryType)
        {
            using (var command = CreateCommand(query, queryType))
            {
                try
                {
                    return command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    //Serilog.Log.Error(ex, "Error executing non-query: {Query}", query);
                    return -1;
                }
            }
        }

        public List<T> ExecutePaginatedQuery<T>(string query, int pageIndex, int pageSize, Func<SqlDataReader, T> mapFunction)
        {
            List<T> result = new List<T>();
            string paginatedQuery = $"{query} OFFSET {pageIndex * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
            using (var command = CreateCommand(paginatedQuery, QueryTypeEnum.SelectQuery))
            {
                try
                {
                    using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            result.Add(mapFunction(reader));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Exception exception = new Exception(ex.Message);
                    throw exception;
                }
            }
            return result;
        }
        public IDataReader ExecuteReader(string query, QueryTypeEnum queryType)
        {
            var command = CreateCommand(query, queryType);
            try
            {
                return command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                command.Dispose();
                throw;
            }
        }
        public IEnumerable<T> ExecuteReaderLazy<T>(string query, QueryTypeEnum queryType, SqlParameter[] parameters, Func<SqlDataReader, T> mapFunction)
        {
            using (SqlConnection connection = _connectionManager.GetConnection())
            {
                using (SqlCommand command = CreateCommand(query, queryType))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            yield return mapFunction(reader);
                        }
                    }
                }
            }
        }
        public List<T> ExecuteReaderSP<T>(string storedProcedureName, SqlParameter[] parameters, Func<SqlDataReader, T> mapFunction)
        {
            List<T> results = new List<T>();
            try
            {
                using (SqlConnection connection = _connectionManager.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 600;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        command.Prepare();
                        using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            while (reader.Read())
                            {
                                T item = mapFunction(reader);
                                results.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Exception exception = new Exception(ex.Message);
                throw exception;
            }
            return results;
        }
        public bool ExecuteScalar(string query, ref object dbResults, QueryTypeEnum queryType)
        {
            using (var command = CreateCommand(query, queryType))
            {
                try
                {
                    dbResults = command.ExecuteScalar();
                    return true;
                }
                catch (Exception ex)
                {
                    //Serilog.Log.Error(ex, "Error executing scalar query: {Query}", query);
                    return false;
                }
            }
        }

        public DataSet ExecuteSP(string storedProcedureName, object[] _params)
        {
            DataSet dataSet = new DataSet();
            try
            {
                using (var connection = _connectionManager.GetConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        SqlCommandBuilder.DeriveParameters(command);
                        if (_params != null && _params.Length > 0)
                        {
                            int parameterCount = command.Parameters.Count - 1;
                            for (int i = 0; i < parameterCount && i < _params.Length; i++)
                            {
                                command.Parameters[i + 1].Value = _params[i] ?? DBNull.Value;
                            }
                        }
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(dataSet);
                        }
                    }
                }
                return dataSet;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while executing the stored procedure '{storedProcedureName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        ///  method can be used for procedure like where procedure is only returning an single table / this is beneficial then dataset 
        /// </summary>
        /// <param name="storedProcedureName"></param>
        /// <param name="_params"></param>
        /// <param name="resultTable"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool ExecuteSPToDataTable(string storedProcedureName, object[] _params, ref DataTable resultTable)
        {
            try
            {
                using (var connection = _connectionManager.GetConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        SqlCommandBuilder.DeriveParameters(command);
                        if (_params != null && _params.Length > 0)
                        {
                            int parameterCount = command.Parameters.Count - 1;
                            for (int i = 0; i < parameterCount && i < _params.Length; i++)
                            {
                                command.Parameters[i + 1].Value = _params[i] ?? DBNull.Value;
                            }
                        }
                        command.Prepare();
                        using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.CloseConnection))
                        {
                            resultTable.Load(reader);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while executing the stored procedure '{storedProcedureName}': {ex.Message}", ex);
            }
        }
        public bool ExecuteStoredProcedureToDataTable(string storedProcedureName, SqlParameter[] parameters, ref DataTable resultTable)
        {
            resultTable = new DataTable();
            try
            {
                using (SqlConnection connection = _connectionManager.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 600;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        command.Prepare();
                        using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.CloseConnection))
                        {
                            resultTable.Load(reader);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }
        }
        public bool ExecuteStoredProcedureWithOutput(string storedProcedureName, SqlParameter[] parameters, ref object outputValue)
        {
            outputValue = null;
            try
            {
                using (SqlConnection connection = _connectionManager.GetConnection())
                {
                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 600;
                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }
                        SqlParameter outputParam = new SqlParameter("@OutputParam", SqlDbType.NVarChar)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputParam);
                        command.ExecuteNonQuery();
                        outputValue = command.Parameters["@OutputParam"].Value;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string GetConnectionStatistics()
        {
            using (var connection = _connectionManager.GetConnection())
            {
                connection.Open();
                var stats = $"Connection State: {connection.State}\n";
                stats += $"Connection Timeout: {connection.ConnectionTimeout} seconds\n";
                stats += $"Database: {connection.Database}\n";
                stats += $"Data Source: {connection.DataSource}";
                return stats;
            }
        }

        private SqlCommand CreateCommand(string query, QueryTypeEnum queryType)
        {
            var connection = _connectionManager.GetConnection();
            var command = connection.CreateCommand();
            command.CommandText = query;
            command.CommandType = queryType == QueryTypeEnum.StandardStoredProcedure ||
                                  queryType == QueryTypeEnum.UserDefinedStoredProcedure
                ? CommandType.StoredProcedure
                : CommandType.Text;
            return command;
        }
        private bool IsTransient(SqlException ex)
        {
            // Logic to determine if the exception is transient
            return ex.Number == -2 || // Timeout
                   ex.Number == 4060 || // Cannot open database
                   ex.Number == 40101; // Connection issues
        }
    }
}
