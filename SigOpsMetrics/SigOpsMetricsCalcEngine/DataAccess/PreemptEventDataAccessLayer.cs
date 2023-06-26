using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using MySqlConnector;
using Parquet;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    public class PreemptEventDataAccessLayer : BaseDataAccessLayer
    {

        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["PREEMPT_TABLE_NAME"];
        private static readonly string? MySqlPreemptTableName = ConfigurationManager.AppSettings["PREEMPT_EVENT_TABLE_NAME"];

        #region Write to MySQL
        public static async Task<bool> GetWritePreemptSignalsToDb(IEnumerable<PreemptSignalModel> preempts)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Timestamp", typeof(DateTime));
            dataTable.Columns.Add("signalID", typeof(long));
            dataTable.Columns.Add("EventCode", typeof(long));
            dataTable.Columns.Add("EventParam", typeof(long));

            // Populate the DataTable with events data
            foreach (var eventData in preempts)
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

        public static async Task<bool> GetWritePreemptEventsToDb(IEnumerable<PreemptModel> preempts)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("InputOn", typeof(DateTime));
            dataTable.Columns.Add("EntryStart", typeof(DateTime));
            dataTable.Columns.Add("TrackClear", typeof(DateTime));
            dataTable.Columns.Add("InputOff", typeof(DateTime));
            dataTable.Columns.Add("DwellService", typeof(DateTime));
            dataTable.Columns.Add("ExitCall", typeof(DateTime));
            dataTable.Columns.Add("SignalID", typeof(long));
            dataTable.Columns.Add("Duration", typeof(long));
            dataTable.Columns.Add("PreemptType", typeof(string));
            dataTable.Columns.Add("ExternalCallOn", typeof(bool));
            dataTable.Columns.Add("ExternalCallOff", typeof(bool));


            // Populate the DataTable with events data
            foreach (var eventData in preempts)
            {
                dataTable.Rows.Add(
                eventData.InputOn,
                eventData.EntryStart,
                eventData.TrackClear,
                eventData.InputOff,
                eventData.DwellService,
                eventData.ExitCall,
                eventData.SignalID,
                TimeSpan.FromTicks(eventData.Duration.Ticks).TotalSeconds,
                eventData.PreemptType,
                eventData.ExternalCallOn,
                eventData.ExternalCallOff
            );
            }
            // Open a connection to MySQL
            try
            {
                return await MySqlWriter(MySqlPreemptTableName ?? throw new InvalidOperationException(), dataTable);
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

        #region Read from AmazonS3

        public static async Task<List<PreemptSignalModel>> ProcessPreemptSignals(DateTime startDate, DateTime endDate,
            List<long?>? signalIdList = null, List<long?>? eventCodes = null)
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
                            var preemptData = await ParquetConvert.DeserializeAsync<PreemptSignalModel>(ms);
                            return signalIdList != null && eventCodes == null
                                ? preemptData.Where(x => signalIdList.Contains(x.SignalID)).ToList()
                                : eventCodes != null && signalIdList == null
                                    ? preemptData.Where(x => eventCodes.Contains(x.EventCode)).ToList()
                                    : preemptData.Where(x =>
                                            signalIdList != null && eventCodes != null &&
                                            eventCodes.Contains(x.EventCode) && signalIdList.Contains(x.SignalID))
                                        .ToList();
                        }
                        catch
                        {
                            await WriteToErrorLog("SigOpsMetricsCalcEngine.PreemptEventDataAccessLayer", $"ProcessPreemptEvents at sensor {obj.Key}", new Exception("Error reading from S3"));
                            return new List<PreemptSignalModel>();

                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    var results = await Task.WhenAll(tasks);
                    foreach (var preemptList in results)
                    {
                        PreemptEvents.AddRange(preemptList);
                    }
                    startDate = startDate.AddDays(1);

                }
                return PreemptEvents;

            }

            #region Error Handling


            catch (Exception e)
            {
                await WriteToErrorLog("PreemptEventDataAccessLayer", "ProcessPreemptSignals", e);
                throw;
            }
            #endregion
        }
        public static async Task<bool> ProcessPreemptSignalsOverload(DateTime startDate, DateTime endDate, List<long?>? signalIdList = null)
        {
            var t = await ProcessPreemptSignals(startDate, endDate, eventCodes: new List<long?> { 102,105,106,104,107,111,707,708 }, signalIdList: signalIdList);

            return await GetWritePreemptSignalsToDb(t);

        }

        #endregion

        public static async Task<List<PreemptSignalModel>> ReadAllFromMySql()
        {
            var events = new List<PreemptSignalModel>();
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
                    var preemptEvent = new PreemptSignalModel
                    {
                        Timestamp = reader.GetDateTime("Timestamp"),
                        SignalID = reader.GetInt64("signalID"),
                        EventCode = reader.GetInt64("EventCode"),
                        EventParam = reader.GetInt64("EventParam")
                    };
                    events.Add(preemptEvent);
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
