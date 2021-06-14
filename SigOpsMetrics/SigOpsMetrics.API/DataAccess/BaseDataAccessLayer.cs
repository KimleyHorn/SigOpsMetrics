using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;

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
    }
}
