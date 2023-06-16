using System.Configuration;
using System.Data;
using Amazon.S3;
using Amazon.S3.Model;
using MySqlConnector;
using Parquet;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    public class FlashEventDataAccessLayer :BaseDataAccessLayer
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
               return await MySqlHelper(MySqlTableName ?? throw new InvalidOperationException(), dataTable);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                //await WriteToErrorLog(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name, "toMySQL", e);
                throw;
            }
        }

        public static async Task WriteFlashPairsToDb(IEnumerable<FlashPairModel> events)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Start", typeof(DateTime));
            dataTable.Columns.Add("End", typeof(DateTime));
            dataTable.Columns.Add("signalID", typeof(long));
            dataTable.Columns.Add("duration", typeof(long));
            dataTable.Columns.Add("startParam", typeof(long));


            // Populate the DataTable with events data
            foreach (var eventData in events)
            {
                    dataTable.Rows.Add(
                    eventData.FlashStart.Timestamp,
                    eventData.FlashEnd.Timestamp,
                    eventData.SignalId,
                    TimeSpan.FromTicks(eventData.FlashDuration.Ticks).TotalSeconds,
                    eventData.StartParam
                );
            }
            // Open a connection to MySQL
            try
            {
                MySqlConnection.Open();

                var bulkCopy = new MySqlBulkCopy(MySqlConnection)
                {
                    DestinationTableName = $"{MySqlDbName}.{MySqlFlashPairTableName}"
                };

                await bulkCopy.WriteToServerAsync(dataTable);

                await MySqlConnection.CloseAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e.ToString());
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
        /// <param name="signalIdList">A list of signal Ids to be retrieved</param>
        /// <param name="eventCodes"></param>
        /// <returns>A List of Flash _events that can be used to write to the flash_event_log server</returns>

        public static async Task<List<FlashEventModel>> ProcessFlashEvents(DateTime startDate, DateTime endDate, List<long?>? signalIdList = null, List<long?>? eventCodes = null)
        {

            if (endDate == default)
            {
                endDate = startDate;
            }

            if (startDate > endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            try
            {

                using var client = new AmazonS3Client(AwsAccess, AwsSecret, BucketRegion);
                while (startDate <= endDate)
                {
                    var s3Objects = await GetListRequest(client, startDate);
                    var semaphore = new SemaphoreSlim(ThreadCount);

                    //For debugging purposes to speed up data processing
                    //#if DEBUG
                    //                    var elementToKeep = s3Objects[0]; // Choose the element you want to keep

                    //                    s3Objects.RemoveAll(obj => obj != elementToKeep);

                    //#endif
                    var tasks = s3Objects.Select(async obj =>
                    {
                        await semaphore.WaitAsync();

                        try
                        {

                            using var response = await MemoryStreamHelper(obj, client);
                            using var ms = new MemoryStream();
                            await response.ResponseStream.CopyToAsync(ms);

                            var flashData = await ParquetConvert.DeserializeAsync<FlashEventModel>(ms);
                            return signalIdList != null && eventCodes == null
                                ? flashData.Where(x => signalIdList.Contains(x.SignalID)).ToList()
                                : eventCodes != null && signalIdList == null
                                    ? flashData.Where(x => eventCodes.Contains(x.EventCode)).ToList()
                                    : flashData.Where(x =>
                                            signalIdList != null && eventCodes != null &&
                                            eventCodes.Contains(x.EventCode) && signalIdList.Contains(x.SignalID))
                                        .ToList();
                        }
                        catch {
                            await WriteToErrorLog("SigOpsMetricsCalcEngine.BaseDataAccessLayer", $"ProcessFlashEvents at sensor {obj.Key}", new Exception("Error reading from S3"));
                        return new List<FlashEventModel>();

                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    var results = await Task.WhenAll(tasks);
                    foreach (var flashList in results)
                    {
                        FlashEvents.AddRange(flashList);
                    }
                    startDate = startDate.AddDays(1);

                }
                return FlashEvents;

            }

            #region Error Handling


            catch (Exception e)
            {
                await WriteToErrorLog("FlashEventDataAccessLayer", "ProcessFlashEvents", e);
                throw;
            }
            #endregion
        }


        public static async Task<bool> ProcessFlashEventsOverload(DateTime startDate, DateTime endDate, List<long?>? signalIdList = null)
        {
            var t = await ProcessFlashEvents(startDate, endDate, eventCodes: new List<long?> { 173 }, signalIdList: signalIdList);

            return await WriteFlashEventsToDb(t);
            
        }
        public static async Task<List<FlashEventModel>> ReadAllFromMySql()
        {
            var events = new List<FlashEventModel>();
            try
            {
                if (MySqlConnection.State == ConnectionState.Closed)
                {
                    await MySqlConnection.OpenAsync();
                }

                await using var cmd = new MySqlCommand($"SELECT t.* FROM {MySqlDbName}.{MySqlTableName} t", MySqlConnection);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var flashEvent = new FlashEventModel
                    {
                        Timestamp = reader.GetDateTime("Timestamp"),
                        SignalID = reader.GetInt64("signalID"),
                        EventCode = reader.GetInt64("EventCode"),
                        EventParam = reader.GetInt64("EventParam"),
                        DeviceID = reader.GetInt64("DeviceID"),
                        __index_level_0__ = reader.GetInt64("IndexLevel")
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


    }
}
