using System;
using System.Configuration;
using System.Windows.Forms;
using SpineAnalyzer.Common;

namespace SpineAnalyzer
{
    /// <summary>
    /// Database configuration and setup helper
    /// </summary>
    public static class DatabaseSetup
    {
        /// <summary>
        /// Configure AppData with database settings
        /// </summary>
        /// <param name="appData">AppData instance to configure</param>
        /// <param name="server">SQL Server instance name</param>
        /// <param name="database">Database name</param>
        /// <param name="useWindowsAuth">Use Windows authentication (true) or SQL auth (false)</param>
        /// <param name="username">SQL username (if using SQL auth)</param>
        /// <param name="password">SQL password (if using SQL auth)</param>
        public static void ConfigureDatabase(AppData appData, string server, string database, 
            bool useWindowsAuth = true, string username = "", string password = "")
        {
            appData.SQLServer = server;
            appData.SQLDatabase = database;
            appData.SQLAuthSQL = useWindowsAuth ? "false" : "true";
            appData.SQLUser = username;
            appData.SQLPassword = password;
        }

        /// <summary>
        /// Setup with typical development settings
        /// </summary>
        /// <param name="appData">AppData instance to configure</param>
        public static void SetupDevelopmentDatabase(AppData appData)
        {
            // Common development settings
            ConfigureDatabase(
                appData: appData,
                server: @".\SQLEXPRESS",  // Local SQL Server Express
                database: "SpineAnalyzer",
                useWindowsAuth: true
            );
        }

        /// <summary>
        /// Test database connectivity
        /// </summary>
        /// <param name="appData">Configured AppData instance</param>
        /// <returns>True if connection successful</returns>
        public static bool TestDatabaseConnection(AppData appData)
        {
            try
            {
                var database = new DataBase(
                    appData.SQLServer, 
                    appData.SQLDatabase, 
                    appData.SQLAuthSQL, 
                    appData.SQLUser, 
                    appData.SQLPassword
                );
                
                return database.TestConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection failed: {ex.Message}", 
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Setup sample user for testing
        /// </summary>
        /// <param name="appData">AppData instance to configure</param>
        public static void SetupTestUser(AppData appData)
        {
            appData.globalUser = new User(
                firstName: "Test",
                lastName: "User", 
                userName: "testuser",
                userID: "testuser",
                canDownload: true,
                canDelete: true,
                canOnlySeeOwn: false
            );

            appData.localStudyUser = new User(
                firstName: "Study",
                lastName: "User",
                userName: "studyuser", 
                userID: "studyuser",
                canDownload: true,
                canDelete: false,
                canOnlySeeOwn: true
            );
        }
    }
}