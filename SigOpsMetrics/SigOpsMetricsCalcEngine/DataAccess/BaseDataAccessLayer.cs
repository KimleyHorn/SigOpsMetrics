using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MySqlConnector;
using Parquet;
using SigOpsMetricsCalcEngine.Models;
using System.Configuration;
using System.Data;

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
        internal List<BaseEventLogModel> SignalEvents = [];
        internal static readonly string MySqlDbName = ConfigurationManager.AppSettings["DB_NAME"] ?? "mark1";
        internal static readonly string? MySqlConnString = ConfigurationManager.AppSettings["CONN_STRING"];
        internal static MySqlConnection MySqlConnection;

        static BaseDataAccessLayer()
        {
            MySqlConnection = new MySqlConnection(MySqlConnString);
        }

        #region Helper Methods

        /// <summary>
        /// A private helper method that will return a list of S3Objects from a given date
        /// </summary>
        /// <param name="client">An AmazonS3Client that is created to handle the requests from the S3 server</param>
        /// <param name="startDate">The date associated with the request from the S3 server</param>
        /// <returns>A list of S3 objects from a given day</returns>
        private static async Task<List<S3Object>> GetListRequest(AmazonS3Client client, DateTime startDate)
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = AwsBucketName,
                Prefix = $"{FolderName}/date={startDate:yyyy-MM-dd}/{FolderName}_"
            };
            var res = await client.ListObjectsV2Async(listRequest);
            return res.S3Objects.ToList();
        }

        /// <summary>
        /// A helper method that handles writing the entire DataTable to a MySQL table using MySqlBulkCopy
        /// </summary>
        /// <param name="mySqlTableName">The name of the MySQL table derived from the app.config file</param>
        /// <param name="dataTable">A collection of objects to be written to the MySQL table</param>
        /// <returns>True if operation was successful</returns>
        internal static async Task<bool> MySqlWriter(string mySqlTableName, DataTable dataTable)
        {
            //Write a conditional statement that returns true if the data was written to the table successfully

            try
            {
                await MySqlConnection.OpenAsync();
#if DEBUG
                Console.WriteLine("Connection Opened");
#endif
                var bulkCopy = new MySqlBulkCopy(MySqlConnection)
                {
                    DestinationTableName = $"{MySqlDbName}.{mySqlTableName}"
                };
#if DEBUG
                Console.WriteLine("Bulk Copy Created");
#endif

                await bulkCopy.WriteToServerAsync(dataTable);
#if DEBUG
                Console.WriteLine("Bulk Copy Written");
#endif

                await MySqlConnection.CloseAsync();
#if DEBUG
                Console.WriteLine("Connection Closed");
                Console.WriteLine("Written to Database");
#endif
            }
            catch (NullReferenceException n)
            {
                Console.WriteLine(n + " MySqlConnection object null");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return true;
        }

        /// <summary>
        /// A helper method that will return a GetObjectResponse from a given S3Object
        /// </summary>
        /// <param name="obj">The S3 object that encapsulates the GetObjectResponse</param>
        /// <param name="client">An AmazonS3Client that is created to handle the requests from the S3 server</param>
        /// <returns>A GetObjectResponse from the AmazonS3Object</returns>
        private static async Task<GetObjectResponse> MemoryStreamHelper(S3Object obj, AmazonS3Client client)
        {
            var request = new GetObjectRequest
            {
                BucketName = AwsBucketName,
                Key = obj.Key
            };
            Console.WriteLine(obj.Key + " Requested");
            return await client.GetObjectAsync(request);
        }

        /// <summary>
        /// A helper method that will check the database for a given date range to see if the data is already present. This method will return true if the data is present and false if the data is not present.
        /// </summary>
        /// <param name="mySqlTableName">The name of the MySQL table that the method is checking</param>
        /// <param name="mySqlColName">The column name that contains dates on the MySQL table</param>
        /// <param name="mySqlDbName">The name of the database that the method is checking</param>
        /// <param name="startDate">The first day this method searches for</param>
        /// <param name="endDate">The last day this method searches for</param>
        /// <returns>True if data is present and false if data is not present</returns>
        private async Task<bool> CheckDB(string mySqlTableName, string mySqlColName, string mySqlDbName, DateTime startDate = new DateTime(),
            DateTime endDate = new DateTime())
        {
            if (endDate < startDate)
                throw new ArgumentException("End date cannot be before start date");

            if (startDate == new DateTime())
                startDate = DateTime.Today.AddDays(-1);

            if (endDate == new DateTime())
                endDate = DateTime.Today;

            try
            {
                if (MySqlConnection.State == ConnectionState.Closed)
                    await MySqlConnection.OpenAsync();

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

                while (await reader.ReadAsync())
                {
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
            }
            catch (Exception e)
            {
                await WriteToErrorLog("SigOpsMetricsCalcEngine.BaseDataAccessLayer", "CheckDB", e);
                throw;
            }

            return true;
        }

        /// <summary>
        /// A method that inputs a list of valid dates and a list of signal Ids, event codes, start date, end date, and a table name and returns a list of valid dates that are not present in the database
        /// </summary>
        /// <param name="startDate">The first day this method searches for</param>
        /// <param name="endDate">The last day this method searches for</param>
        /// <param name="eventCodes">A list of event codes this method filters by</param>
        /// <param name="mySqlTableName">The name of the MySQL table that the method is checking</param>
        /// <returns>A list of valid dates that are not present in the database</returns>
        internal async Task<List<DateTime>> FillData(DateTime startDate, DateTime endDate, List<long?> eventCodes, string mySqlTableName)
        {
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                                     .Select(offset => startDate.AddDays(offset));
            await CheckDB(mySqlTableName, "Timestamp", MySqlDbName, startDate, endDate);

            var filteredSignals = allDates
                .Where(date => !SignalEvents.Any(signalEvent => date.Equals(signalEvent.Timestamp) && eventCodes.Contains(signalEvent.EventCode)))
                .ToList();
            return filteredSignals;
        }

        /// <summary>
        /// The method that is used to filter base log event models into flash events and preempt events. This method is flexible and can be used for any event type that is based off of the base log event model
        /// </summary>
        /// <param name="dates">A list of valid dates to filter by</param>
        /// <param name="eventCodes">A list of event codes to filter by</param>
        /// <param name="mySqlTableName">The name of the MySQL table that the method is checking</param>
        /// <param name="mySqlColName">The column name that contains dates on the MySQL table</param>
        /// <returns>A list of BaseEventLogModels to be added to the MySql table</returns>
        internal async Task<List<BaseEventLogModel>> FilterData(List<DateTime> dates, List<long?>? eventCodes, string mySqlTableName, string mySqlColName)
        {
            await CheckDB(mySqlTableName, mySqlColName, MySqlDbName, dates.FirstOrDefault(), dates.LastOrDefault());

            var filteredSignals = SignalEvents
                .Where(signal => eventCodes != null && eventCodes.Contains(signal.EventCode)).ToList();

            return filteredSignals;
        }

        #endregion Helper Methods

        #region Signal Processing

        /// <summary>
        /// The Process Events method will take a list of valid dates and a list of signal Ids and event codes and return a list of events that can be used to write to the flash events server
        /// </summary>
        /// <param name="validDates">A list of valid dates</param>
        /// <param name="signalIdList">A list of signal Ids to be retrieved</param>
        /// <param name="eventCodes"></param>
        /// <returns>A List of Flash _events that can be used to write to the flash event server</returns>
        /// <exception cref="ArgumentException">Thrown when event codes are used without signalIDs</exception>
        public async Task<bool> ProcessEvents(List<DateTime> validDates, List<long?>? signalIdList = null, List<long?>? eventCodes = null)
        {
            try
            {
                using var client = new AmazonS3Client(AwsAccess, AwsSecret, BucketRegion);
                foreach (var date in validDates)
                {
                    var s3Objects = await GetListRequest(client, date);
                    var semaphore = new SemaphoreSlim(ThreadCount);
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
                            return [];
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
                }

                return true;
            }

            #region Error Handling

            catch (Exception e)
            {
                await WriteToErrorLog("FlashEventDataAccessLayer", "ProcessFlashEvents", e);
                throw;
            }

            #endregion Error Handling
        }

        #endregion Signal Processing

        #region Error Logging

        /// <summary>
        /// The overloaded method that will write to the error log
        /// </summary>
        /// <param name="applicationName">The name of the file the error is coming from</param>
        /// <param name="functionName">The name of the function the error is coming from</param>
        /// <param name="ex">The exception being thrown</param>
        /// <returns>A task since the method is asynchronous</returns>
        public static async Task WriteToErrorLog(string applicationName,
     string functionName, Exception ex)
        {
            await WriteToErrorLog(applicationName, functionName, ex.Message,
                ex.InnerException?.ToString());
        }

        /// <summary>
        /// The private method that will write to the error log in the database
        /// </summary>
        /// <param name="applicationName">The name of the file the error is coming from</param>
        /// <param name="functionName">The name of the function the error is coming from</param>
        /// <param name="exception">The exception being thrown</param>
        /// <param name="innerException">The inner exception being thrown (if applicable)</param>
        /// <returns>A task since the method is asynchronous</returns>
        private static async Task WriteToErrorLog(string applicationName,
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

        #endregion Error Logging
    }
}