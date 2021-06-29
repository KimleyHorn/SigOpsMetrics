using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using SigOpsMetrics.API.Classes.DTOs;

namespace SigOpsMetrics.API.DataAccess
{
    public class BaseDataAccessLayer
    {
        public static async Task WriteToErrorLog(MySqlConnection sqlConnection, string applicationName, string functionName, Exception ex)
        {
            await WriteToErrorLog(sqlConnection, applicationName, functionName, ex.Message, ex.InnerException?.ToString());
        }

        public static async Task WriteToErrorLog(MySqlConnection sqlConnection, string applicationName,
            string functionName, string exception, string innerException)
        {
            try
            {
                await sqlConnection.OpenAsync();
                using (var cmd = new MySqlCommand())
                {

                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        $"insert into mark1.errorlog (applicationname, functionname, exception, innerexception) values ('{applicationName}', '{functionName}', '{exception.Substring(0, exception.Length > 500 ? 500 : exception.Length)}', '{innerException}') ";
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public static async Task<int> WriteToContactUs(MySqlConnection sqlConnection, ContactInfo data)
        {
            int success = 0;  
            
            if (CheckInvalidString(data.FirstName) || CheckInvalidString(data.LastName) || CheckInvalidString(data.EmailAddress) || CheckInvalidString(data.Reason) || CheckInvalidString(data.Comments))
            {
                return 0;
            }

            if (data.Comments.Length > 500)
            {
                data.Comments = data.Comments.Substring(0, 500);
            }

            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText = "INSERT INTO mark1.UserComments (FirstName, LastName, EmailAddress, PhoneNumber, Reason, Comments) VALUES (@firstName, @lastName, @emailAddress, @phoneNumber, @reason, @comments)";
                    cmd.Parameters.AddWithValue("firstName", data.FirstName);
                    cmd.Parameters.AddWithValue("lastName", data.LastName);
                    cmd.Parameters.AddWithValue("emailAddress", data.EmailAddress);
                    cmd.Parameters.AddWithValue("phoneNumber", DBHelper(data.PhoneNumber));
                    cmd.Parameters.AddWithValue("reason", data.Reason);
                    cmd.Parameters.AddWithValue("comments", data.Comments);

                    await cmd.ExecuteNonQueryAsync();
                    success = 1;
                }
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "WriteToContactUs", ex);
            }
            finally
            {
                sqlConnection.Close();
            }
            return success;
        }

        private static object DBHelper(string str)
        {
            if (str != null && str.Trim().Length > 0)
            {
                return str.Trim();
            } else
            {
                return DBNull.Value;
            }
        }

        private static bool CheckInvalidString(string str)
        {
            if (str == null)
            {
                return true;
            }
            if (string.IsNullOrEmpty(str.Trim()))
            {
                return true;
            }
            return false;
        }
    }
}
