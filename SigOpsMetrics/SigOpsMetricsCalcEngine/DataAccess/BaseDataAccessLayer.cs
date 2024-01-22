using System.Configuration;
using System.Data;
using System.Diagnostics;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MySqlConnector;
using Parquet;
using SigOpsMetrics.Models;
using SigOpsMetricsCalcEngine.Calcs;

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
        public static List<BaseEventLogModel> FlashEvents = new();
        public static List<BaseEventLogModel> PreemptEvents = new();
        public static List<PreemptModel> PreemptPairs = new();
        internal static List<BaseEventLogModel> SignalEvents = new();
        internal static readonly string MySqlDbName = ConfigurationManager.AppSettings["DB_NAME"] ?? "mark1";
        internal static readonly string? MySqlConnString = ConfigurationManager.AppSettings["CONN_STRING"];
        internal static MySqlConnection? MySqlConnection;


        static BaseDataAccessLayer() {
            MySqlConnection = new MySqlConnection(MySqlConnString);
        }

        //inside of checkdb make sure that it pulls and dumps from both databases

        public async Task<bool> RunFlash(DateTime startDate, DateTime endDate)
        {
            //Preempt event list to check for eventCodes from SignalEvents list
            var flashEventList = new List<long?> {173};

            //This checks to see if the events are already in the database or if they are already stored in memory
            if (! CheckList(startDate, endDate, flashEventList) && !await CheckDB("flash_event_log", "Timestamp", MySqlDbName, startDate, endDate))
            {
                //If events within date range are not within the database or memory, then grab the events from Amazon S3
                return await ProcessEvents(startDate, endDate, "flash_event_log", eventCodes: flashEventList);
            }

            return true;
        }

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

        #endregion

        #region Signal Processing

        /// <summary>
        /// This method will return a list of _events from AWS S3 and fit them to the FlashEventModel class for use in the rest of the solution
        /// </summary>
        /// <param name="startDate">The start date of the _events to be retrieved</param>
        /// <param name="endDate">The end date of the _events to be retrieved</param>
        /// <param name="signalIdList">A list of signal Ids to be retrieved</param>
        /// <param name="eventCodes"></param>
        /// <returns>A List of Flash _events that can be used to write to the flash_event_log server</returns>

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
