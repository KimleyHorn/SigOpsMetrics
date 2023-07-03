using System.Configuration;
using System.Data;
using Amazon.S3;
using Amazon.S3.Model;
using MySqlConnector;
using Parquet;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    public class FlashEventDataAccessLayer : BaseDataAccessLayer
    {

        
        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["FLASH_EVENT_TABLE_NAME"];
        private static readonly string? MySqlFlashPairTableName = ConfigurationManager.AppSettings["FLASH_PAIR_TABLE_NAME"];


        #region Write to MySQL
        public static async Task<bool> WriteFlashEventsToDb(IEnumerable<FlashEventModel> events)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Timestamp", typeof(DateTime));
            dataTable.Columns.Add("signalID", typeof(long));
            dataTable.Columns.Add("EventCode", typeof(long));
            dataTable.Columns.Add("EventParam", typeof(long));

            // Populate the DataTable with events data
            foreach (var eventData in events)
            {
                dataTable.Rows.Add(
                    eventData.Timestamp,
                    eventData.SignalID,
                    eventData.EventCode,
                    eventData.EventParam

                );
            }
            // Open a connection to MySQL
            try
            {
                return await MySqlWriter(MySqlTableName ?? throw new InvalidOperationException(), dataTable);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                //await WriteToErrorLog(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name, "toMySQL", e);
                throw;
            }
        }

        public static async Task<bool> WriteFlashPairsToDb(IEnumerable<FlashPairModel> events)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Start", typeof(DateTime));
            dataTable.Columns.Add("End", typeof(DateTime));
            dataTable.Columns.Add("signalID", typeof(long));
            dataTable.Columns.Add("duration", typeof(long));
            dataTable.Columns.Add("startParam", typeof(long));
            dataTable.Columns.Add("IsOpen", typeof(bool));


            // Populate the DataTable with events data
            foreach (var eventData in events)
            {
                if (eventData.FlashEnd != null)
                    dataTable.Rows.Add(
                        eventData.FlashStart,
                        eventData.FlashEnd,
                        eventData.SignalID,
                        TimeSpan.FromTicks(eventData.FlashDuration.Ticks).TotalSeconds,
                        eventData.StartParam,
                        eventData.IsOpen
                    );
                else if (eventData.FlashEnd == null)
                    dataTable.Rows.Add(
                        eventData.FlashStart, 
                        DBNull.Value,
                        eventData.SignalID,
                        TimeSpan.FromTicks(eventData.FlashDuration.Ticks).TotalSeconds,
                        eventData.StartParam,
                        eventData.IsOpen
                                                                  );
            }
            // Open a connection to MySQL
            try
            {
                return await MySqlWriter(MySqlFlashPairTableName ?? throw new InvalidOperationException(), dataTable);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                var applicationName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
                if (applicationName != null)
                    await WriteToErrorLog(applicationName, "toMySQL", e);
                throw;
            }
        }
        #endregion




        /// <summary>
        /// This method will return a list of _events from AWS S3 and fit them to the FlashEventModel class for use in the rest of the solution
        /// </summary>
        /// <param name="startDate">The start date of the _events to be retrieved</param>
        /// <param name="endDate">The end date of the _events to be retrieved</param>
        /// <returns>A List of Flash _events that can be used to write to the flash_event_log server</returns>

        public static async Task<bool> ProcessFlashEvents(DateTime startDate, DateTime endDate)
        {
            await ConvertToFlash(SignalEvents,startDate, endDate);
            return await WriteFlashEventsToDb(FlashEvents);
        }

        public static Task ConvertToFlash(List<BaseSignalModel> signalList, DateTime startDate, DateTime endDate)
            {
                FlashEvents = new List<FlashEventModel>();

                foreach (var flash in signalList.Where(x => x.EventCode is 173 && x.Timestamp >= startDate && x.Timestamp <= endDate))
                {


                    FlashEvents.Add(new FlashEventModel
                    {
                        SignalID = flash.SignalID,
                        Timestamp = flash.Timestamp,
                        EventCode = flash.EventCode,
                        EventParam = flash.EventParam
                    });
                }

                return Task.CompletedTask;
            }




        public static async Task<List<FlashEventModel>> ReadAllFromMySql()
        {
            var events = new List<FlashEventModel>();
            try
            {

                await using var reader = await MySqlReader(MySqlTableName, MySqlDbName);


                while (await reader.ReadAsync())
                {
                    var flashEvent = new FlashEventModel
                    {
                        Timestamp = reader.GetDateTime("Timestamp"),
                        SignalID = reader.GetInt64("signalID"),
                        EventCode = reader.GetInt64("EventCode"),
                        EventParam = reader.GetInt64("EventParam"),
                    };
                    events.Add(flashEvent);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally { await MySqlConnection.CloseAsync(); }

            return events;
        }        
        public static async Task<List<string>> ReadAllEventsFromMySql(MySqlConnection? sqlConnection = null)
        {

            sqlConnection ??= MySqlConnection;
            
            var events = new List<FlashPairModel>();
            try
            {

                await using var reader = await MySqlReader(MySqlFlashPairTableName, MySqlDbName, sqlConnection);


                while (await reader.ReadAsync())
                {
                    var flashEvent = new FlashPairModel
                    {
                        FlashStart = reader.GetDateTime("Start"),
                        FlashEnd = reader.GetDateTime("End"),
                        SignalID = reader.GetInt64("SignalID"),
                        FlashDuration = TimeSpan.FromSeconds(reader.GetInt64("duration")),
                        StartParam = reader.GetInt64("EventParam"),
                        IsOpen = reader.GetBoolean("IsOpen")
                    };
                    events.Add(flashEvent);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                await MySqlConnection.CloseAsync();
            }

            var flashString = events.Select(flashEvent => flashEvent.ToString()).ToList();
            return flashString;
        }


    }
}
