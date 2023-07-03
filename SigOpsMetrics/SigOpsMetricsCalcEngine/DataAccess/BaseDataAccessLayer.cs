using System.Configuration;
using System.Data;
using System.Diagnostics;
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
        internal static List<FlashEventModel> FlashEvents = new();
        internal static List<PreemptSignalModel> PreemptEvents = new();
        internal static List<BaseSignalModel> SignalEvents = new();
        internal static readonly string? MySqlDbName = ConfigurationManager.AppSettings["DB_NAME"];
        internal static readonly string? MySqlConnString = ConfigurationManager.AppSettings["CONN_STRING"];
        internal static MySqlConnection? MySqlConnection;

        static BaseDataAccessLayer() {
            MySqlConnection = new MySqlConnection(MySqlConnString);
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
            MySqlConnection.Open();
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

        internal static async Task<MySqlDataReader> MySqlReader(string mySqlTableName, string mySqlDbName, MySqlConnection? mySqlConnection = null)
        {
            mySqlConnection ??= MySqlConnection;
            

            if (mySqlConnection.State == ConnectionState.Closed)
            {
                await mySqlConnection.OpenAsync();
            }

            await using var cmd = new MySqlCommand($"SELECT t.* FROM {mySqlDbName}.{mySqlTableName} t", MySqlConnection);
            await using var reader = await cmd.ExecuteReaderAsync();
            return reader;
        }

        public static async Task<bool> CheckDB(string mySqlTableName, string mySqlDbName, DateTime startDate,
            DateTime endDate)
        {
            //var reader = await MySqlReader(mySqlTableName, mySqlDbName);

            if (MySqlConnection.State == ConnectionState.Closed)
            {
                await MySqlConnection.OpenAsync();
            }

            await using var cmd = new MySqlCommand($"SELECT * FROM {mySqlDbName}.{mySqlTableName} WHERE Timestamp BETWEEN '{startDate}' AND '{endDate}'", MySqlConnection);
            await using var reader = await cmd.ExecuteReaderAsync();

            if (!reader.HasRows)
            {
                await CheckList(startDate, endDate);
                await reader.CloseAsync();
                return false;
            }


            while (await reader.ReadAsync()) {
                var flashEvent = new BaseSignalModel
                {
                    Timestamp = reader.GetDateTime("Timestamp"),
                    SignalID = reader.GetInt64("signalID"),
                    EventCode = reader.GetInt64("EventCode"),
                    EventParam = reader.GetInt64("EventParam")
                };
            SignalEvents.Add(flashEvent);
            }

            await MySqlConnection.CloseAsync();
            return true;

        }

        internal static async Task<List<BaseSignalModel>> CheckList(DateTime startDate,
            DateTime endDate)
        {
            var dateSignal = SignalEvents.Where(x => x.Timestamp >= startDate && x.Timestamp <= endDate).ToList();

            if (dateSignal.Count == 0)
            {
                await ProcessEvents(startDate, endDate);
            }
            return dateSignal;
        }


        //Abstraction 1 (Unfinished)
        internal static async Task<MemoryStream> ResponseMaker(S3Object obj, AmazonS3Client client)
        {
            using var response = await MemoryStreamHelper(obj, client);
            using var ms = new MemoryStream();
            await response.ResponseStream.CopyToAsync(ms);
            return ms;
        }

        //Abstraction 2 (Unfinished)
        internal static List<BaseSignalModel> SignalFilter(List<long?>? signalIdList, List<long?>? eventCodes, List<BaseSignalModel> signalData)
        {
                return signalIdList != null && eventCodes == null
        ? signalData.Where(x => signalIdList.Contains(x.SignalID)).ToList()
        : eventCodes != null && signalIdList == null
            ? signalData.Where(x => eventCodes.Contains(x.EventCode)).ToList()
            : signalData.Where(x => signalIdList != null && eventCodes != null && eventCodes.Contains(x.EventCode) && signalIdList.Contains(x.SignalID)).ToList();
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

        public static async Task ProcessEvents(DateTime startDate, DateTime endDate, List<long?>? signalIdList = null, List<long?>? eventCodes = null)
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
                            var signalData = await ParquetConvert.DeserializeAsync<BaseSignalModel>(ms);



                            switch (signalIdList, eventCodes)
                            {
                                case (null, null):
                                    return signalData.ToList();
                                    
                                case (null, not null):
                                    throw new ArgumentException("Event Codes cannot be used without Signal Ids");

                                case (not null, null):
                                    return signalData.Where(x => signalIdList.Contains(x.SignalID)).ToList();
                                   
                                case (not null, not null):
                                    return signalData.Where(x => signalIdList.Contains(x.SignalID) && eventCodes.Contains(x.EventCode)).ToList();
                               
                            }


                        }
                        catch
                        {
                            //await WriteToErrorLog("SigOpsMetricsCalcEngine.BaseDataAccessLayer", $"ProcessFlashEvents at sensor {obj.Key}", new Exception("Error reading from S3"));
                            return new List<BaseSignalModel>();

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
