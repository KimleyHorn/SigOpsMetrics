
using System.Configuration;
using System.Data;
using System.Runtime.CompilerServices;
using MySqlConnector;
using SigOpsMetricsCalcEngine.Calcs;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    public static class FlashEventDataAccessLayer
    {
        // #GlobalVariables
        private static readonly string? MySqlConnString = ConfigurationManager.AppSettings["CONN_STRING"];
        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["TABLE_NAME"];
        private static readonly string? MySqlDBName = ConfigurationManager.AppSettings["DB_NAME"];
        internal static MySqlConnection? MySqlConnection;

        static FlashEventDataAccessLayer()
        {
            MySqlConnection = new MySqlConnection(MySqlConnString);
        }

        public static async Task WriteFlashEventsToDb(IEnumerable<FlashEventModel> events)
        {
            // Create a DataTable to hold the events data
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Timestamp", typeof(DateTime));
            dataTable.Columns.Add("signalID", typeof(long));
            dataTable.Columns.Add("EventCode", typeof(long));
            dataTable.Columns.Add("EventParam", typeof(long));
            dataTable.Columns.Add("DeviceID", typeof(long));
            dataTable.Columns.Add("IndexLevel", typeof(long));

            // Populate the DataTable with events data
            foreach (var eventData in events)
            {
                dataTable.Rows.Add(
                    eventData.Timestamp,
                    eventData.SignalID,
                    eventData.EventCode,
                    eventData.EventParam,
                    eventData.DeviceID,
                    eventData.__index_level_0__
                );
            }
            // Open a connection to MySQL
            try
            {
                MySqlConnection.Open();

                var bulkCopy = new MySqlBulkCopy(MySqlConnection)
                {
                    DestinationTableName = $"{MySqlDBName}.{MySqlTableName}"
                };

                await bulkCopy.WriteToServerAsync(dataTable);

                await MySqlConnection.CloseAsync();
            }
            catch (MySqlException e)
            {
                Console.WriteLine("Wrong Password");
                await WriteToErrorLog(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name, "toMySQL", e);
                throw;
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Connection Timeout");
                await WriteToErrorLog(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name, "toMySQL", e);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e.ToString());
                await WriteToErrorLog(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name, "toMySQL", e);
                throw;
            }
        }

        public static async Task WriteToErrorLog(string applicationName,
            string functionName, Exception ex)
        {
            await WriteToErrorLog(applicationName, functionName, ex.Message,
                ex.InnerException?.ToString());
        }

        public static async Task WriteToErrorLog(string applicationName,
            string functionName, string exception, string innerException)
        {
            try
            {
                if (MySqlConnection.State == ConnectionState.Closed)
                {
                    await MySqlConnection.OpenAsync();
                }
                await using var cmd = new MySqlCommand();

                cmd.Connection = MySqlConnection;
                cmd.CommandText =
                    $"insert into {MySqlDBName}.errorlog (applicationname, functionname, exception, innerexception) values ('{applicationName}', '{functionName}', '{exception.Substring(0, exception.Length > 500 ? 500 : exception.Length)}', '{innerException}') ";
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally { await MySqlConnection.CloseAsync(); }

        }

    }
}
