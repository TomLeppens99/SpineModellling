using System;
using System.IO;

namespace SpineAnalyzer.Common
{
    /// <summary>
    /// Centralized application data and configuration class
    /// Contains all settings, database connections, and user information
    /// </summary>
    public class AppData
    {
        // SQL Database properties
        public string SQLAuthSQL { get; set; }
        public string SQLDatabase { get; set; }
        public string SQLPassword { get; set; }
        public string SQLServer { get; set; }
        public string SQLUser { get; set; }

        // File system directories
        public string TempDir { get; set; } = Path.GetTempPath();
        public string DataDirectory { get; set; } = "Data";
        public string ConfigFile { get; set; } = "config.json";
        public string AcquisitionDir { get; set; } = "Acquisitions";

        // User management
        public User globalUser { get; set; } = new User();
        public User localStudyUser { get; set; } = new User();

        // Constructor with default values
        public AppData()
        {
            // Initialize with safe defaults
            globalUser = new User();
            localStudyUser = new User();
        }
    }

    /// <summary>
    /// User information class
    /// Contains user details and permissions
    /// </summary>
    public class User
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string _UserName { get; set; } = string.Empty;
        public string _UserID { get; set; } = string.Empty;
        public bool _CanDownload { get; set; } = false;
        public bool _CanDelete { get; set; } = false;
        public bool _CanOnlySeeOwnEOSmeasurements { get; set; } = false;

        // Constructor with default values
        public User()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            _UserName = string.Empty;
            _UserID = string.Empty;
            _CanDownload = false;
            _CanDelete = false;
            _CanOnlySeeOwnEOSmeasurements = false;
        }

        // Constructor with parameters
        public User(string firstName, string lastName, string userName, string userID, bool canDownload = false, bool canDelete = false, bool canOnlySeeOwn = false)
        {
            FirstName = firstName;
            LastName = lastName;
            _UserName = userName;
            _UserID = userID;
            _CanDownload = canDownload;
            _CanDelete = canDelete;
            _CanOnlySeeOwnEOSmeasurements = canOnlySeeOwn;
        }
    }
}