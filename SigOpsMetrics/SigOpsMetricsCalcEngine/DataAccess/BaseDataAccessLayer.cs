using System.Configuration;
using System.Data;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MySqlConnector;
using Parquet;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    public class BaseDataAccessLayer
    {
        internal static readonly string? AwsAccess = ConfigurationManager.AppSettings["S3_ACCESS_KEY"];
        internal static readonly string? AwsSecret = ConfigurationManager.AppSettings["S3_SECRET_KEY"];
        internal static readonly string? AwsBucketName = ConfigurationManager.AppSettings["S3_BUCKET_NAME"];
        internal static readonly RegionEndpoint? BucketRegion = RegionEndpoint.USEast1;
        internal static readonly string? FolderName = ConfigurationManager.AppSettings["FOLDER_NAME"];
        internal static readonly int ThreadCount = int.Parse(ConfigurationManager.AppSettings["THREAD_COUNT"] ?? "1");
        private static List<BaseEventLogModel> _flashEvents = new();
        private static List<BaseEventLogModel> _preemptEvents = new();
        internal static List<BaseEventLogModel> SignalEvents = new();
        internal static readonly string MySqlDbName = ConfigurationManager.AppSettings["DB_NAME"] ?? "mark1";
        internal static readonly string? MySqlConnString = ConfigurationManager.AppSettings["CONN_STRING"];
        internal static MySqlConnection? MySqlConnection;


        static BaseDataAccessLayer() {
            MySqlConnection = new MySqlConnection(MySqlConnString);
        }


        #region Getters and Setters

        public static List<BaseEventLogModel> FlashEvents()
        {
            return _flashEvents;
        }

        public static void AddFlash(BaseEventLogModel f)
        { 
            _flashEvents.Add(f);
        }

        public static void AddPreempt(BaseEventLogModel f)
        {
            _preemptEvents.Add(f);
        }

        public static List<BaseEventLogModel> PreemptEvents()
        {
            return _preemptEvents;
        }

        #endregion

        #region Helper Methods

        internal static async Task<List<S3Object>> GetListRequest(AmazonS3Client client, DateTime startDate)
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = AwsBucketName,
                Prefix = $"{FolderName}/date={startDate:yyyy-MM-dd}/{FolderName}_"
            };
            var res = await client.ListObjectsV2Async(listRequest);
            return res.S3Objects.ToList();

        }

        internal static async Task<bool> MySqlWriter(string mySqlTableName, DataTable dataTable)
        {
            //Write a conditional statement that returns true if the data was written to the table successfully
            await MySqlConnection.OpenAsync();
            Console.WriteLine("Connection Opened");
            var bulkCopy = new MySqlBulkCopy(MySqlConnection)
            {
                DestinationTableName = $"{MySqlDbName}.{mySqlTableName}"
            };
            Console.WriteLine("Bulk Copy Created");
            await bulkCopy.WriteToServerAsync(dataTable);
            Console.WriteLine("Bulk Copy Written");
            await MySqlConnection.CloseAsync();
            Console.WriteLine("Connection Closed");
            Console.WriteLine("Written to Database");
            return true;
        }

        internal static async Task<GetObjectResponse> MemoryStreamHelper(S3Object obj, AmazonS3Client client)
        {
            var request = new GetObjectRequest
            {
                BucketName = AwsBucketName,
                Key = obj.Key
            };
            Console.WriteLine(obj.Key + " Requested");
            return await client.GetObjectAsync(request);
        }


        public static async Task<bool> CheckDB(string mySqlTableName, string mySqlColName, string mySqlDbName, DateTime startDate,
            DateTime endDate)
        {
            if (MySqlConnection.State == ConnectionState.Closed)
            {
                await MySqlConnection.OpenAsync();
            }

            await using var cmd = MySqlConnection.CreateCommand();
            cmd.CommandText = $"SELECT * FROM {mySqlDbName}.{mySqlTableName} WHERE {mySqlColName} BETWEEN @StartDate AND @EndDate";
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);

            await using var reader = await cmd.ExecuteReaderAsync();



            if (!reader.HasRows)
            {
                await MySqlConnection.CloseAsync();
                return false;
            }


            while (await reader.ReadAsync()) {
                var signalEvent = new BaseEventLogModel
                {
                    Timestamp = reader.GetDateTime("Timestamp"),
                    SignalID = reader.GetInt64("signalID"),
                    EventCode = reader.GetInt64("EventCode"),
                    EventParam = reader.GetInt64("EventParam")
                };
            SignalEvents.Add(signalEvent);
            }

            await MySqlConnection.CloseAsync(); 
            return true;

        }

        internal static bool CheckList(DateTime startDate,
            DateTime endDate, List<long?> eventCodes)
        {
            var dateSignal = SignalEvents.Where(x => x.Timestamp >= startDate && x.Timestamp <= endDate && eventCodes.Contains(x.EventCode)).ToList();
            return dateSignal.Count != 0;
        }


        internal static List<DateTime> FillData(DateTime startDate, DateTime endDate, List<long?> eventCodes)
        {
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                                     .Select(offset => startDate.AddDays(offset));

            var dateList = allDates
                .Where(date => !SignalEvents.Any(signalEvent => date.Equals(signalEvent.Timestamp) && eventCodes.Contains(signalEvent.EventCode)))
                .ToList();

            return dateList;
        }

        internal static async Task<DateTime> GetLastDay(string mySqlTableName, string mySqlColName, string mySqlDbName)
        {
            if (MySqlConnection.State == ConnectionState.Closed)
            {
                await MySqlConnection.OpenAsync();
            }

            await using var cmd = MySqlConnection.CreateCommand();
            cmd.CommandText = $"SELECT MAX({mySqlColName}) AS latest_timestamp FROM {mySqlDbName}.{mySqlTableName}";
            var latestTimestamp = DateTime.MinValue;
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                await MySqlConnection.CloseAsync();
                return latestTimestamp;
            }

            
            while (await reader.ReadAsync())
            {
                latestTimestamp = reader.GetDateTime("latest_timestamp");
                Console.WriteLine($"Latest Timestamp: {latestTimestamp}");
            }
            await MySqlConnection.CloseAsync();
            return latestTimestamp;
        }

        public static bool HasGaps(List<DateTime> dateTimes)
        {
            // Sort the list of DateTimes
            dateTimes.Sort();

            if (dateTimes.Max() == dateTimes.Min())
            {
                return true;
            }

            // Check for gaps between consecutive DateTimes
            for (var i = 0; i < dateTimes.Count - 1; i++)
            {
                var gap = dateTimes[i + 1] - dateTimes[i];
                if (gap.TotalDays > 1)
                {
                    // Adjust the condition based on your definition of a gap
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Signal Processing

        /// <summary>
        /// This method will return a list of _events from AWS S3 and fit them to the FlashEventModel class for use in the rest of the solution
        /// </summary>
        /// <param name="startDate">The start date of the _events to be retrieved</param>
        /// <param name="endDate">The end date of the _events to be retrieved</param>
        /// <param name="signalIdList">A list of signal Ids to be retrieved</param>
        /// <param name="eventCodes"></param>
        /// <returns>A List of Flash _events that can be used to write to the flash event server</returns>

        public static async Task<bool> ProcessEvents(DateTime startDate, DateTime endDate, string mySqlTableName, List<long?>? signalIdList = null, List<long?>? eventCodes = null)
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
                            //Abstraction 1
                            using var response = await MemoryStreamHelper(obj, client);
                            using var ms = new MemoryStream();
                            await response.ResponseStream.CopyToAsync(ms);


                            //Abstraction 2
                            var signalData = await ParquetConvert.DeserializeAsync<BaseEventLogModel>(ms);

                            return (signalIdList, eventCodes) switch
                            {
                                (null, null) => signalData.ToList(),
                                (null, not null) => throw new ArgumentException(
                                    "Event Codes cannot be used without Signal Ids"),
                                (not null, null) => signalData.Where(x => signalIdList.Contains(x.SignalID)).ToList(),
                                (not null, not null) => signalData.Where(x =>
                                        signalIdList.Contains(x.SignalID) && eventCodes.Contains(x.EventCode))
                                    .ToList()
                            };
                        }
                        catch
                        {
                            //await WriteToErrorLog("SigOpsMetricsCalcEngine.BaseDataAccessLayer", $"ProcessFlashEvents at sensor {obj.Key}", new Exception("Error reading from S3"));
                            return new List<BaseEventLogModel>();

                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    var results = await Task.WhenAll(tasks);
                    foreach (var sigList in results)
                    {
                        SignalEvents.AddRange(sigList);
                    }
                    startDate = startDate.AddDays(1);

                }

                return true;
            }

            #region Error Handling


            catch (Exception e)
            {
                await WriteToErrorLog("FlashEventDataAccessLayer", "ProcessFlashEvents", e);
                throw;
            }
            #endregion
            

        }

        /// <summary>
        /// The Process Events method will take a list of valid dates and a list of signal Ids and event codes and return a list of events that can be used to write to the flash events server
        /// </summary>
        /// <param name="validDates">A list of valid dates</param>
        /// <param name="signalIdList">A list of signal Ids to be retrieved</param>
        /// <param name="eventCodes"></param>
        /// <returns>A List of Flash _events that can be used to write to the flash event server</returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<bool> ProcessEvents(List<DateTime> validDates, string mySqlTableName, List<long?>? signalIdList = null, List<long?>? eventCodes = null)
        {
            try
            {
                using var client = new AmazonS3Client(AwsAccess, AwsSecret, BucketRegion);
                foreach (var date in validDates)
                {
                    var s3Objects = await GetListRequest(client, date);
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
                            //Abstraction 1
                            using var response = await MemoryStreamHelper(obj, client);
                            using var ms = new MemoryStream();
                            await response.ResponseStream.CopyToAsync(ms);


                            //Abstraction 2
                            var signalData = await ParquetConvert.DeserializeAsync<BaseEventLogModel>(ms);

                            return (signalIdList, eventCodes) switch
                            {
                                (null, null) => signalData.ToList(),
                                (null, not null) => throw new ArgumentException(
                                    "Event Codes cannot be used without Signal Ids"),
                                (not null, null) => signalData.Where(x => signalIdList.Contains(x.SignalID)).ToList(),
                                (not null, not null) => signalData.Where(x =>
                                        signalIdList.Contains(x.SignalID) && eventCodes.Contains(x.EventCode))
                                    .ToList()
                            };
                        }
                        catch
                        {
                            //await WriteToErrorLog("SigOpsMetricsCalcEngine.BaseDataAccessLayer", $"ProcessFlashEvents at sensor {obj.Key}", new Exception("Error reading from S3"));
                            return new List<BaseEventLogModel>();

                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    });

                    var results = await Task.WhenAll(tasks);
                    foreach (var sigList in results)
                    {
                        SignalEvents.AddRange(sigList);
                    }
                    //startDate = startDate.AddDays(1);

                }

                return true;
            }

            #region Error Handling


            catch (Exception e)
            {
                await WriteToErrorLog("FlashEventDataAccessLayer", "ProcessFlashEvents", e);
                throw;
            }
            #endregion


        }

        #endregion

        #region Error Logging
        public static async Task WriteToErrorLog(string applicationName,
     string functionName, Exception ex)
        {
            await WriteToErrorLog(applicationName, functionName, ex.Message,
                ex.InnerException?.ToString());
        }
        public static async Task WriteToErrorLog(string applicationName,
            string functionName, string exception, string? innerException)
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
                    $"insert into {MySqlDbName}.errorlog (applicationname, functionname, exception, innerexception) values ('{applicationName}', '{functionName}', " +
                    $"'{exception[..(exception.Length > 500 ? 500 : exception.Length)]}', " +
                    $"'{innerException}') ";
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally { await MySqlConnection.CloseAsync(); }

        }
        #endregion
    }
}
