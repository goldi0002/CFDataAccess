using CFDataAccess.SQLServer.Utils;
using System.Data;
using System.Data.SqlClient;

namespace CFDataAccess.SQLServer
{
    public interface IDataAccess : IDisposable
    {
        /// <summary>
        /// Executes a scalar query (typically one that returns a single value) and returns the result.
        /// </summary>
        /// <param name="query">The SQL query or stored procedure name to execute.</param>
        /// <param name="dbResults">An output parameter for holding the query result.</param>
        /// <param name="queryType">The type of query being executed, based on the <see cref="QueryType"/>.</param>
        /// <returns>True if the query executes successfully; otherwise, false.</returns>
        bool ExecuteScalar(string query, ref object dbResults, QueryTypeEnum queryType);
        /// <summary>
        /// Executes a non-query SQL command (e.g., INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="query">The SQL query or stored procedure name to execute.</param>
        /// <param name="queryType">The type of query being executed, based on the <see cref="QueryTypeEnum"/>.</param>
        /// <returns>The number of affected rows.</returns>
        int ExecuteNonQuery(string query, QueryTypeEnum queryType);
        /// <summary>
        /// Executes a SQL query or stored procedure that returns a set of data (e.g., SELECT).
        /// </summary>
        /// <param name="query">The SQL query or stored procedure name to execute.</param>
        /// <param name="queryType">The type of query being executed, based on the <see cref="QueryTypeEnum"/>.</param>
        /// <returns>A data reader object that can be used to read the result set.</returns>
        IDataReader ExecuteReader(string query, QueryTypeEnum queryType);
        bool ExecuteStoredProcedureToDataTable(string storedProcedureName, SqlParameter[] parameters, ref DataTable resultTable);
        List<T> ExecuteReaderSP<T>(string storedProcedureName, SqlParameter[] parameters, Func<SqlDataReader, T> mapFunction);
        bool ExecuteStoredProcedureWithOutput(string storedProcedureName, SqlParameter[] parameters, ref object outputValue);
        IEnumerable<T> ExecuteReaderLazy<T>(string query, QueryTypeEnum queryType, SqlParameter[] parameters, Func<SqlDataReader, T> mapFunction);
        /// <summary>
        ///  Fetch data in chunks,which is especially used for large result sets. pass your query without adding these lines of code 
        ///  OFFSET {pageIndex * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="mapFunction"></param>
        /// <returns></returns>
        List<T> ExecutePaginatedQuery<T>(string query, int pageIndex, int pageSize, Func<SqlDataReader, T> mapFunction);
        public bool ExecuteBatch(List<(string query, SqlParameter[] parameters)> queries, QueryTypeEnum queryType);
        bool ExecuteSPToDataTable(string storedProcedureName, object[] _params, ref DataTable resultTable);
        DataSet ExecuteSP(string storedProcedureName, object[] _params);
        bool BulkInsert(DataTable _dTable, string destinationTableName);
        string GetConnectionStatistics();
    }
}
