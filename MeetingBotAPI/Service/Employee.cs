using MeetingBotAPI.DataAccess;
using MeetingBotAPI.Interface;
using Microsoft.Data.SqlClient;
using System.Data;

namespace MeetingBotAPI.Service
{
    public class Employee: IEmployee
    {
        private IConfiguration Configuration;
        public Employee(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        DatabaseConnection dbConnection;


        public DataTable checkUserLogin(string userid)
        {
            DataTable table = new DataTable();
            string ConnString = this.Configuration.GetConnectionString("Default");
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnString))
                {
                    SqlCommand command = new SqlCommand("SPLogin_Check", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@username", userid);
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    da.Fill(table);
                }
            }
            catch (Exception ex)
            {

            }
            return table;

        }





        public DataTable CheckUpdateUserBlocked(int EmployeeId, bool IsAuthenticated)
        {
            DataTable result;
            try
            {
                string ConnString = this.Configuration.GetConnectionString("Default");

                // passing connection string 
                dbConnection = new SqlConnectionManager(ConnString);

                // Create a dictionary of parameters with their values
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@EmployeeId", EmployeeId },
                    { "@IsAuthenticated", IsAuthenticated }
                };


                result = dbConnection.ExecuteParameterizedQuery("SPLogin_CheckUpdateUserBlocked", parameters);
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }

    }
}
