using System.Data.SqlClient;
using System.Data;
using System;
using System.Collections.Generic;

namespace MeetingBotAPI.DataAccess
{
    // Concrete class implementing SQL Server database connection
    public class SqlConnectionManager : DatabaseConnection
    {
        public SqlConnectionManager(string connectionString) : base(connectionString)
        {
        }

        // Implementation of ExecuteQuery method for SQL Server
        public override DataTable ExecuteQuery(string query)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                DataTable dataTable = new DataTable();
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dataTable);
                    }

                }
                catch (Exception ex)
                {
                    // Handle exception
                    Console.WriteLine("Error executing query: " + ex.Message);
                }
                return dataTable;
            }
        }

        // Implementation of ExecuteParameterizedQuery method for SQL Server
        public override DataTable ExecuteParameterizedQuery(string query, Dictionary<string, object> parameters)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                DataTable dataTable = new DataTable();
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        // Add parameters
                        foreach (var parameter in parameters)
                        {
                            command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                        }
                        //command.Parameters.AddRange(parameters);
                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        adapter.Fill(dataTable);
                    }
                }
                catch (Exception ex)
                {
                    // Handle exception
                    Console.WriteLine("Error executing parameterized query: " + ex.Message);
                }
                return dataTable;
            }
        }
    }
}
