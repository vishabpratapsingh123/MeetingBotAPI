using MeetingBotAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingBotAPI
{
    //public class GlobalSetting
    //{
    //}

    public static class Extensions
    {
        public static string ToStringFromNull(this object obj)
        {
            if (obj == System.DBNull.Value)
            {
                return "";
            }
            else if (obj == null)
            {
                return "";
            }
            else
            {
                try
                {
                    return obj.ToString().Trim();
                }
                catch
                {
                    return "";
                }
            }
        }
        public static bool ToboolFromNull(this object obj)
        {
            if (obj == System.DBNull.Value)
            {
                return false;
            }
            else if (obj == null)
            {
                return false;
            }
            else
            {
                try
                {
                    if ((bool)obj)
                        return true;
                    else
                        return false;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static int ToIntFromNull(this object obj)
        {
            if (obj == System.DBNull.Value)
            {
                return 0;
            }
            else if (obj == null)
            {
                return 0;
            }
            else
            {
                try
                {
                    return Convert.ToInt32(obj);
                }
                catch
                {
                    return 0;
                }
            }
        }
        public static decimal To2Decimal(this object obj)
        {
            if (obj == System.DBNull.Value)
            {
                return 0;
            }
            else if (obj == null)
            {
                return 0;
            }
            else
            {
                try
                {
                    return Math.Round(Convert.ToDecimal(obj.ToString().Replace("\r", "").Replace("\a", "").Trim()), 2);
                }
                catch
                {
                    return 0;
                }
            }
        }
    }

    public static class GlobalSetting
    {
        //LoginDetails
        #region
        private static IConfiguration _configure;
        public static int CurrentSessionId = 0;
        public static int LoggedInUserId = 1;
        //public static string USERNAME = "";
        public static int ActiveSessionId = 0;
        public static string ActiveSessionName = "";

        //public static int LoginDesignationID = 0;   // 1 is for Admin

        #endregion
        //LoginDetails

        #region Dsetting Tag name

        public static string DSettingTag_AuditSession = "AuditSession";
        public static string DSettingTag_CurrentSession = "CURRENTSESSION";
        public static string DSettingTag_PreviousSessionName = "PreviousSession";
        #endregion

        //TableName
        #region
        public static string tbAirportName = "[Master].[tblAirport]";
        public static string tbAirportName_ClientID = "ClientID";
        #endregion
        //TableName




        public static void Reset()
        { }

        public static List<T> ConvertInToList<T>(DataTable dt)
        {
            var fields = typeof(T).GetFields();

            List<T> lst = new List<T>();

            foreach (DataRow dr in dt.Rows)
            {
                // Create the object of T
                var ob = Activator.CreateInstance<T>();

                foreach (var fieldInfo in fields)
                {
                    foreach (DataColumn dc in dt.Columns)
                    {
                        // Matching the columns with fields
                        if (fieldInfo.Name == dc.ColumnName)
                        {
                            // Get the value from the datatable cell
                            object value = dr[dc.ColumnName];

                            // Set the value into the object
                            fieldInfo.SetValue(ob, value);
                            break;
                        }
                    }
                }

                lst.Add(ob);
            }

            return lst;
        }


        public static string ApplicationOpen_Setting(string year)
        {
            DataTable dataTable = new DataTable();
            //var temp = ConfigurationManager.ConnectionStrings[""].ConnectionString;
            string sessionId = "";
            using (SqlConnection con = new SqlConnection("Data Source=15.237.12.204,5967;Initial Catalog=Meeting_Bot;User Id:Garima;Password:Chandigarh1!"))
            {
                if (con.State == ConnectionState.Closed)
                    con.Open();
                SqlCommand command = new SqlCommand("SELECT RID FROM [Setting].[DSetting] where isActive =1 and [value] ='" + year + "'", con);
                command.CommandType = System.Data.CommandType.Text;
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dataTable);
            }
            foreach (DataRow da in dataTable.Rows)
            {
                sessionId = da["RID"].ToString();
            }
            return sessionId;
        }


        ////Get Previous Session
        //if (con.State == ConnectionState.Closed) con.Open();
        //    check_DSetting = new SqlCommand(@"SELECT top 1 RID,Value FROM [Setting].[DSetting] WHERE ([Tag] =@Tag and IsActive='1')", con);
        //    check_DSetting.Parameters.AddWithValue("@Tag", GlobalSetting.DSettingTag_PreviousSessionName);
        //    reader = check_DSetting.ExecuteReader();
        //    if (reader.HasRows)
        //    {
        //        while (reader.Read())
        //        {
        //            HttpContext.Current.Session["PreviousSessionID"] = reader.GetInt32(0);
        //            HttpContext.Current.Session["PreviousSessionName"] = reader.GetString(1);
        //        }
        //        reader.Close();
        //    }
        //}




        public static bool IsSiteWorking()
        {
            //ConnectionString consre = ConnectionString.GetInstance;
            bool IsSiteWorking = false;
            using (SqlConnection con = new SqlConnection("Data Source=15.237.12.204,5967;Initial Catalog=Meeting_Bot;User Id:Garima;Password:Chandigarh1!"))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                SqlCommand check_DSetting = new SqlCommand("SELECT top 1 * FROM [Setting].[CheckMaintenance]", con);
                SqlDataReader reader = check_DSetting.ExecuteReader();
                if (reader.HasRows)
                {
                    //int SessionId = 0;
                    while (reader.Read())
                    {
                        IsSiteWorking = reader.GetBoolean(1);
                    }
                    reader.Close();
                }
            }
            return IsSiteWorking;
        }
        public static bool IsSiteWorkingForAdmin()
        {

            bool IsSiteWorking = false;
            using (SqlConnection con = new SqlConnection("Data Source=15.237.12.204,5967;Initial Catalog=Meeting_Bot;User Id:Garima;Password:Chandigarh1!"))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                SqlCommand check_DSetting = new SqlCommand("SELECT top 1 * FROM [Setting].[CheckMaintenance] where IsOpenSiteForAdmin='1'", con);
                SqlDataReader reader = check_DSetting.ExecuteReader();
                if (reader.HasRows)
                {
                    //int SessionId = 0;
                    while (reader.Read())
                    {
                        IsSiteWorking = reader.GetBoolean(1);
                    }
                    reader.Close();
                }
            }

            return IsSiteWorking;
        }
        public static bool IsSiteWorkingForNonAdmin()
        {
            bool IsSiteWorking = false;
            using (SqlConnection con = new SqlConnection("Data Source=15.237.12.204,5967;Initial Catalog=Meeting_Bot;User Id:Garima;Password:Chandigarh1!"))
            {
                if (con.State == ConnectionState.Closed) con.Open();
                SqlCommand check_DSetting = new SqlCommand("SELECT top 1 * FROM [Setting].[CheckMaintenance] where IsOpenSiteForAll='1'", con);
                SqlDataReader reader = check_DSetting.ExecuteReader();
                if (reader.HasRows)
                {
                    //int SessionId = 0;
                    while (reader.Read())
                    {
                        IsSiteWorking = reader.GetBoolean(1);
                    }
                    reader.Close();
                }
            }

            return IsSiteWorking;
        }

        public static string TimeAgo(this DateTime dateTime)
        {
            string result = string.Empty;
            var timeSpan = DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                result = string.Format("{0} seconds ago", timeSpan.Seconds);
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                result = timeSpan.Minutes > 1 ?
                    String.Format("about {0} minutes ago", timeSpan.Minutes) :
                    "about a minute ago";
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                result = timeSpan.Hours > 1 ?
                    String.Format("about {0} hours ago", timeSpan.Hours) :
                    "about an hour ago";
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                result = timeSpan.Days > 1 ?
                    String.Format("about {0} days ago", timeSpan.Days) :
                    "yesterday";
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                result = timeSpan.Days > 30 ?
                    String.Format("about {0} months ago", timeSpan.Days / 30) :
                    "about a month ago";
            }
            else
            {
                result = timeSpan.Days > 365 ?
                    String.Format("about {0} years ago", timeSpan.Days / 365) :
                    "about a year ago";
            }

            return result;
        }
    }
}

