﻿using System;
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
}
