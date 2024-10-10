using System;
using System.Data.SqlClient;

namespace CFDataAccess.SQLServer
{
    public class ConnectionManager : IDisposable
    {
        private readonly string _connectionString;
        private SqlConnection _connection;
        private bool _disposed;

        public ConnectionManager(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new SqlConnection(_connectionString);
        }

        public void OpenConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    _connection.Open();
                }
                catch (SqlException ex)
                {
                    throw new InvalidOperationException("Could not open a connection to the database.", ex);
                }
            }
        }

        public void CloseConnection()
        {
            if (_connection.State == System.Data.ConnectionState.Open)
            {
                try
                {
                    _connection.Close();
                }
                catch (SqlException ex)
                {
                    throw new InvalidOperationException("Could not close the connection to the database.", ex);
                }
            }
        }

        public SqlConnection GetConnection()
        {
            return _connection; // Return the connection without opening it
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
            _disposed = true;
        }

        ~ConnectionManager()
        {
            Dispose(false);
        }
    }
    public static class BuildSqlProvider
    {
        /// <summary>
        /// Builds a SQL Server connection string with required parameters.
        /// </summary>
        /// <param name="strServerName">The name of the SQL Server.</param>
        /// <param name="strUserName">The username for the connection.</param>
        /// <param name="strPassword">The password for the connection.</param>
        /// <param name="strDatabase">The name of the database.</param>
        /// <param name="timeout">The connection timeout in seconds.</param>
        /// <returns>The constructed SQL connection string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are null or empty.</exception>
        public static string GetProvider(
            string strServerName,
            string strUserName,
            string strPassword,
            string strDatabase,
            int timeout = 600)
        {
            if (string.IsNullOrWhiteSpace(strServerName))
                throw new ArgumentNullException(nameof(strServerName), "Server name cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(strUserName))
                throw new ArgumentNullException(nameof(strUserName), "Username cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(strPassword))
                throw new ArgumentNullException(nameof(strPassword), "Password cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(strDatabase))
                throw new ArgumentNullException(nameof(strDatabase), "Database name cannot be null or empty.");

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = strServerName,
                InitialCatalog = strDatabase,
                UserID = strUserName,
                Password = strPassword,
                ConnectTimeout = timeout
            };

            return builder.ConnectionString;
        }

        /// <summary>
        /// Builds a SQL Server connection string with optional encryption.
        /// </summary>
        /// <param name="strServerName">The name of the SQL Server.</param>
        /// <param name="strUserName">The username for the connection.</param>
        /// <param name="strPassword">The password for the connection.</param>
        /// <param name="strDatabase">The name of the database.</param>
        /// <param name="timeout">The connection timeout in seconds.</param>
        /// <param name="encrypt">Indicates whether to encrypt the connection.</param>
        /// <param name="trustServerCertificate">Indicates whether to trust the server certificate.</param>
        /// <returns>The constructed SQL connection string.</returns>
        public static string GetProviderWithEncryption(
            string strServerName,
            string strUserName,
            string strPassword,
            string strDatabase,
            int timeout = 600,
            bool encrypt = false,
            bool trustServerCertificate = false)
        {
            var connectionString = GetProvider(strServerName, strUserName, strPassword, strDatabase, timeout);
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                Encrypt = encrypt,
                TrustServerCertificate = trustServerCertificate
            };

            return builder.ConnectionString;
        }

        /// <summary>
        /// Builds a SQL Server connection string using Integrated Security (Windows Authentication).
        /// </summary>
        /// <param name="strServerName">The name of the SQL Server.</param>
        /// <param name="strDatabase">The name of the database.</param>
        /// <param name="timeout">The connection timeout in seconds.</param>
        /// <returns>The constructed SQL connection string.</returns>
        public static string GetProviderWithIntegratedSecurity(
            string strServerName,
            string strDatabase,
            int timeout = 600)
        {
            if (string.IsNullOrWhiteSpace(strServerName))
                throw new ArgumentNullException(nameof(strServerName), "Server name cannot be null or empty.");
            if (string.IsNullOrWhiteSpace(strDatabase))
                throw new ArgumentNullException(nameof(strDatabase), "Database name cannot be null or empty.");

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = strServerName,
                InitialCatalog = strDatabase,
                IntegratedSecurity = true,
                ConnectTimeout = timeout
            };

            return builder.ConnectionString;
        }
    }
}
