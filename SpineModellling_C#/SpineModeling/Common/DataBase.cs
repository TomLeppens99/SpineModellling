using System;
using System.Data;
using System.Data.SqlClient;

namespace SpineAnalyzer.Common
{
    /// <summary>
    /// Database access layer for SQL Server operations
    /// Handles connections, commands, and data retrieval
    /// </summary>
    public class DataBase
    {
        private string connectionString;

        /// <summary>
        /// Constructor for SQL Server authentication
        /// </summary>
        /// <param name="server">SQL Server instance</param>
        /// <param name="database">Database name</param>
        /// <param name="authSQL">Use SQL authentication (true) or Windows authentication (false)</param>
        /// <param name="username">SQL username (if using SQL auth)</param>
        /// <param name="password">SQL password (if using SQL auth)</param>
        public DataBase(string server, string database, string authSQL, string username, string password)
        {
            if (string.IsNullOrEmpty(server) || string.IsNullOrEmpty(database))
            {
                throw new ArgumentException("Server and database parameters cannot be null or empty");
            }

            // Build connection string based on authentication type
            if (authSQL == "true" && !string.IsNullOrEmpty(username))
            {
                // SQL Server authentication
                connectionString = $"Server={server};Database={database};User Id={username};Password={password};";
            }
            else
            {
                // Windows authentication
                connectionString = $"Server={server};Database={database};Integrated Security=true;";
            }
        }

        /// <summary>
        /// Constructor with direct connection string
        /// </summary>
        /// <param name="connectionString">Complete connection string</param>
        public DataBase(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        /// <summary>
        /// Execute a SQL command and fill a DataSet with results
        /// </summary>
        /// <param name="command">SQL command to execute</param>
        /// <param name="dataSet">DataSet to fill with results</param>
        /// <returns>True if successful</returns>
        public bool ReadDataSet(SqlCommand command, ref DataSet dataSet)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    command.Connection = connection;
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dataSet);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in ReadDataSet: {ex.Message}");
                throw new Exception($"Database operation failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Execute a non-query SQL command (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="command">SQL command to execute</param>
        /// <returns>Number of rows affected</returns>
        public int ExecuteNonQuery(SqlCommand command)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    command.Connection = connection;
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in ExecuteNonQuery: {ex.Message}");
                throw new Exception($"Database operation failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Execute a SQL command and return a single value
        /// </summary>
        /// <param name="command">SQL command to execute</param>
        /// <returns>First column of first row, or null</returns>
        public object ExecuteScalar(SqlCommand command)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    command.Connection = connection;
                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error in ExecuteScalar: {ex.Message}");
                throw new Exception($"Database operation failed: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Test the database connection
        /// </summary>
        /// <returns>True if connection is successful</returns>
        public bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection test failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the current connection string (without password for security)
        /// </summary>
        /// <returns>Connection string with password masked</returns>
        public string GetConnectionInfo()
        {
            // Return connection string with password masked for security
            var builder = new SqlConnectionStringBuilder(connectionString);
            builder.Password = "****";
            return builder.ConnectionString;
        }
    }
}