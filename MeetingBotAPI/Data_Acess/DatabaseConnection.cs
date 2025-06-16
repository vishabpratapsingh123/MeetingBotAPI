using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MeetingBotAPI.DataAccess
{
    // Abstract class for database connection
    public abstract class DatabaseConnection
    {
        protected string ConnectionString { get; }

        // Constructor to initialize connection string
        protected DatabaseConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        // Abstract method for executing a query
        public abstract DataTable ExecuteQuery(string query);

        // Abstract method for executing a parameterized query
        public abstract DataTable ExecuteParameterizedQuery(string query, Dictionary<string, object> parameters);
    }
}
