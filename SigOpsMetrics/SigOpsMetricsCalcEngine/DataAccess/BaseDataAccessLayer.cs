using System.Collections.Concurrent;
using System.Collections.Immutable;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MySqlConnector;
using Parquet;
using System.Configuration;
using System.Data;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Routing.Constraints;
using SigOpsMetricsCalcEngine.Core.Extensions;
using SigOpsMetricsCalcEngine.Core.Models;
using SigOpsMetricsCalcEngine.Core.Helper;

namespace SigOpsMetricsCalcEngine.Core.DataAccess
{
    public class BaseDataAccessLayer
    {
        internal static readonly string? AwsAccess = ConfigurationManager.AppSettings["S3_ACCESS_KEY"];
        internal static readonly string? AwsSecret = ConfigurationManager.AppSettings["S3_SECRET_KEY"];
        internal static readonly string? AwsBucketName = ConfigurationManager.AppSettings["S3_BUCKET_NAME"];
        internal static readonly RegionEndpoint? BucketRegion = RegionEndpoint.USEast1;
        internal static readonly string? FolderName = ConfigurationManager.AppSettings["FOLDER_NAME"];
        internal static readonly int ThreadCount = int.Parse(ConfigurationManager.AppSettings["THREAD_COUNT"] ?? "1");
        public ConcurrentBag<BaseEventLogModel> SignalEvents = [];
        internal static readonly string MySqlDbName = ConfigurationManager.AppSettings["DB_NAME"] ?? "mark1";
        internal static readonly string? MySqlConnString = ConfigurationManager.AppSettings["CONN_STRING"];
        internal static MySqlConnection MySqlConnection;
        internal static ErrorLogger _logger;
        private static readonly string fileName = GetCurrentFileName();
        private static readonly string filePath = StartupOps.directoryPath;

        public BaseDataAccessLayer()
        {
            try
            {
                _logger = new ErrorLogger("Test");
                MySqlConnection = new MySqlConnection(MySqlConnString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MySqlConnection = new MySqlConnection(null);

                _logger.WriteToErrorLogAsync(fileName, "constructor", ex, LogLevel.Error)
                    .ConfigureAwait(false)
                    .GetAwaiter().GetResult();
            }
        }

        #region Helper Methods

        /// <summary>
        /// A private helper method that will return a list of S3Objects from a given date
        /// </summary>
        /// <param name="client">An AmazonS3Client that is created to handle the requests from the S3 server</param>
        /// <param name="startDate">The date associated with the request from the S3 server</param>
        /// <param name="allowedSignalIds">A list of signal ids to include in the list request if needed. Only to be used if needing to get a specific signal or region</param>
        /// <returns>A list of S3 objects from a given day</returns>
        private static async Task<ConcurrentBag<S3Object>> GetListRequest(AmazonS3Client client, DateTime startDate, List<long?> allowedSignalIds = null)
        {
            var allObjects = new ConcurrentBag<S3Object>();
            string continuationToken = null;

            do
            {
                var listRequest = new ListObjectsV2Request
                {
                    BucketName = AwsBucketName,
                    Prefix = $"{FolderName}/date={startDate:yyyy-MM-dd}/{FolderName}_",
                    ContinuationToken = continuationToken
                };


                var res = await client.ListObjectsV2Async(listRequest);

                if (res?.S3Objects == null)
                {
                    break;
                }
                foreach (var obj in res.S3Objects)
                {
                    allObjects.Add(obj);
                }

                //allObjects.AddRange(res.S3Objects);
                continuationToken = res.NextContinuationToken;

            } while (!string.IsNullOrEmpty(continuationToken));

            // If AllowedSignalIds is null, initialize it as an empty list to prevent null reference exceptions
            var useSignals = allowedSignalIds == null;
            allowedSignalIds ??= new List<long?>();

            // Regular expression to extract the signal ID from the filename
            var regex = new Regex(@"atspm_(\d+)_\d{4}-\d{2}-\d{2}\.parquet");



            // Filter objects based on allowed signal IDs
            var filteredObjects = allObjects.Where(obj =>
            {
                if (obj.Key == null)
                {
                    return false; // Skip if the object's Key is null
                }

                var match = regex.Match(obj.Key);
                if (useSignals)
                {
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int signalId))
                    {
                        return allowedSignalIds.Contains(signalId);
                    }
                    return false;
                }
                return true;
            });

            var bagObjects = new ConcurrentBag<S3Object>(filteredObjects);

            return bagObjects;
        }

        /// <summary>
        /// A helper method that handles writing the entire DataTable to a MySQL table using MySqlBulkCopy
        /// </summary>
        /// <param name="mySqlTableName">The name of the MySQL table derived from the app.config file</param>
        /// <param name="dataTable">A collection of objects to be written to the MySQL table</param>
        /// <returns>True if operation was successful</returns>
        public static async Task<bool> MySqlWriter(string mySqlTableName, DataTable dataTable)
        {
            try
            {
                // Ensure the connection is not null before proceeding
                if (MySqlConnection == null)
                {
                    throw new NullReferenceException();
                }

                await StateSwitcherAsync();

                var bulkCopy = new MySqlBulkCopy(MySqlConnection)
                {
                    DestinationTableName = $"{MySqlDbName}.{mySqlTableName}"
                };
#if DEBUG
                Console.WriteLine("Bulk Copy Created.");
#endif

                // Write data from DataTable to the database
                await bulkCopy.WriteToServerAsync(dataTable);

#if DEBUG
                Console.WriteLine("Bulk Copy Written.");
#endif
            }
            catch (NullReferenceException n)
            {
                Console.WriteLine(n + " MySqlConnection object is null.");
                await _logger.WriteToErrorLogAsync(fileName, "MySqlWriter", n);
                return false;
            }
            catch (MySqlException sqlEx)
            {
                Console.WriteLine(sqlEx + " An error occurred with the MySQL connection.");
                await _logger.WriteToErrorLogAsync(fileName, "MySqlWriter", sqlEx);

                // Handle specific MySqlException cases if needed
                switch (sqlEx.Number)
                {
                    
                    // Unable to connect to any of the specified MySQL hosts
                    case 1042:
                        Console.WriteLine("Could not connect to the MySQL server. Check server availability.");
                        break;
                    // Access denied for user
                    case 1045:
                        Console.WriteLine("Access denied. Check your database username and password.");
                        break;
                    // Network-related or instance-specific error
                    case 0:
                        Console.WriteLine("Network-related or instance-specific error. Check network connection.");
                        break;
                }

                return false;
            }
            catch (InvalidOperationException invOpEx)
            {
                Console.WriteLine(invOpEx + " The connection is in an invalid state.");
                await _logger.WriteToErrorLogAsync(fileName, "MySqlWriter", invOpEx);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e + " An unexpected error occurred.");
                await _logger.WriteToErrorLogAsync(fileName, "MySqlWriter", e);
                throw;
            }
            finally
            {
                // Ensure the connection is closed in the end
                if (MySqlConnection != null && MySqlConnection.State != ConnectionState.Closed)
                {
                    await MySqlConnection.CloseAsync();
#if DEBUG
                    Console.WriteLine("Connection closed.");
#endif
                }
            }

            return true;
        }

        /// <summary>
        /// Handles MySQL connection state changes with error handling for timeouts and allows for forced timeouts during testing.
        /// </summary>
        /// <param name="closeConnection">Whether or not the connection needs to be closed. If true, it always closes the connection. If false, it runs through StateSwitcher method</param>
        /// <param name="forceTimeout">Optional parameter to force a connection timeout for testing purposes</param>
        /// <returns>The completed task</returns>
        /// <exception cref="ArgumentOutOfRangeException">If an unknown connection state is identified</exception>
        internal static async Task StateSwitcherAsync(bool closeConnection = false, bool forceTimeout = false)
        {
            try
            {
#if DEBUG
                Console.WriteLine($"Current connection state: {MySqlConnection.State}");
#endif
                if (forceTimeout)
                {
#if DEBUG
                    Console.WriteLine("Force timeout is enabled. Simulating a connection timeout.");
#endif
                    throw new TimeoutException("Simulated MySQL timeout for testing.");
                }

                switch (MySqlConnection.State)
                {
                    case ConnectionState.Broken:
                        await MySqlConnection.CloseAsync();
#if DEBUG
                        Console.WriteLine("Connection was broken. Attempting to close and reopen connection.");
#endif
                        await RetryOnTimeoutAsync(async () => await MySqlConnection.OpenAsync());
#if DEBUG
                        Console.WriteLine("Connection reopened successfully.");
#endif
                        break;
                    case ConnectionState.Closed:
#if DEBUG
                        Console.WriteLine("Connection is closed. Attempting to open connection.");
#endif
                        await RetryOnTimeoutAsync(async () => await MySqlConnection.OpenAsync());
#if DEBUG
                        Console.WriteLine("Connection opened successfully.");
#endif
                        break;
                    case ConnectionState.Connecting:
#if DEBUG
                        Console.WriteLine("Connection is currently being established. Awaiting connection.");
#endif
                        await Task.Delay(500); // Wait for the connection to establish
                        break;
                    case ConnectionState.Open:
#if DEBUG
                        Console.WriteLine("Connection is open. Proceeding with the operation.");
#endif
                        if (closeConnection)
                        {
#if DEBUG
                            Console.WriteLine("Closing connection");
#endif
                            await MySqlConnection.CloseAsync();
                        }

                        break;
                    case ConnectionState.Executing:
#if DEBUG
                        Console.WriteLine("Connection is executing a command.");
#endif
                        break;
                    case ConnectionState.Fetching:
#if DEBUG
                        Console.WriteLine("Connection is fetching data.");
#endif
                        break;
                    default:
#if DEBUG
                        Console.WriteLine("Unknown connection state encountered.");
#endif
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (MySqlException ex) when
                (ex.Number == 1042 || ex.Number == 1158 || ex.Number == 10060) // Timeout error codes
            {
                // Log timeout or take corrective action
#if DEBUG
                Console.WriteLine($"Connection timeout occurred: {ex.Message}");
#endif
                throw new TimeoutException("Connection timed out. Please check your network or database server.", ex);
            }
            catch (TimeoutException te)
            {
                await RetryOnTimeoutAsync(() => Task.FromResult(MySqlConnection.OpenAsync()));

            }
            catch (Exception ex)
            {
                // Log unexpected errors
#if DEBUG
                Console.WriteLine($"Unexpected error: {ex.Message}");
#endif
                throw;
            }
        }

        /// <summary>
        /// Retries a given action if it times out.
        /// </summary>
        /// <param name="action">The asynchronous action to retry.</param>
        /// <param name="maxRetries">Maximum number of retries.</param>
        /// <param name="delayBetweenRetries">Delay between retries in milliseconds.</param>
        private static async Task RetryOnTimeoutAsync(Func<Task> action, int maxRetries = 3, int delayBetweenRetries = 2000)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await action();
                    return;
                }
                catch (MySqlException ex) when (ex.Number == 1042 || ex.Number == 1158 || ex.Number == 10060) // Connection timeout errors
                {
#if DEBUG
                    Console.WriteLine($"Retry {attempt}/{maxRetries} failed: {ex.Message}");
#endif
                    if (attempt == maxRetries)
                        throw; // Rethrow if max retries reached
                    await Task.Delay(delayBetweenRetries);
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static string GetCurrentFileName([CallerFilePath] string filePath = "")
        {
            return Path.GetFileName(filePath);
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
            //Console.WriteLine(obj.Key + " Requested");

            Console.WriteLine($"Requesting object {obj.Key} from bucket {AwsBucketName}");
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
        public static async Task<List<DateTime>> CheckDBAsync(
            string mySqlTableName,
            string mySqlColName,
            string mySqlDbName,
            DateTime startDate = default,
            DateTime endDate = default)
        {
            await StateSwitcherAsync();
            if (endDate < startDate)
                throw new ArgumentException("End date cannot be before start date");

            if (startDate == default)
                startDate = DateTime.Today.AddDays(-1);

            if (endDate == default)
                endDate = DateTime.Today;

            var missingDates = new List<DateTime>();

            try
            {
                if (MySqlConnection.State == ConnectionState.Closed)
                    await MySqlConnection.OpenAsync();

                // Construct the SQL query
                var sql = $@"
            WITH DateRange AS (
                SELECT @StartDate AS Date
                UNION ALL
                SELECT DATE_ADD(Date, INTERVAL 1 DAY)
                FROM DateRange
                WHERE Date < @EndDate
            )
            SELECT d.Date
            FROM DateRange d
            LEFT JOIN {mySqlDbName}.{mySqlTableName} t
            ON d.Date = DATE(t.{mySqlColName})
            WHERE t.{mySqlColName} IS NULL;
        ";

                await using var cmd = MySqlConnection.CreateCommand();
                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);

                // Execute the query
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    missingDates.Add(reader.GetDateTime(0));
                }

                await MySqlConnection.CloseAsync();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("MySQL connection timed out. Please check the connection health.");
                Console.WriteLine(ex);
                await _logger.WriteToErrorLogAsync(fileName, "CheckDB", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while checking the database.");
                Console.WriteLine(ex);
                await _logger.WriteToErrorLogAsync(fileName, "CheckDB", ex);
                throw;
            }

            return missingDates;
        }


        /// <summary>
        /// The method that is used to filter base log event models into flash events and preempt events. This method is flexible and can be used for any event type that is based off of the base log event model
        /// </summary>
        /// <param name="dates">A list of valid dates to filter by</param>
        /// <param name="eventCodes">A list of event codes to filter by</param>
        /// <param name="mySqlTableName">The name of the MySQL table that the method is checking</param>
        /// <param name="mySqlColName">The column name that contains dates on the MySQL table</param>
        /// <returns>A list of BaseEventLogModels to be added to the MySql table</returns>
        internal async Task<ConcurrentBag<BaseEventLogModel>> FilterData(List<DateTime> dates, List<long?>? eventCodes,
            string mySqlTableName, string mySqlColName)
        {
            try
            {
                //var weGood = await CheckDB(mySqlTableName, mySqlColName, MySqlDbName, dates.FirstOrDefault(),
                //    dates.LastOrDefault());
                var filteredSignals = new ConcurrentBag<BaseEventLogModel>(SignalEvents
                    .Where(signal => eventCodes != null && eventCodes.Contains(signal.EventCode)));
                //if (weGood)
                return filteredSignals;
            }
            catch (Exception e)
            {
                await _logger.WriteToErrorLogAsync(fileName, "FilterData", e);
            }

            return [];
        }

        /// <summary>
        /// A helper method that filters a list of BaseEventLogModels by event code so they can be processed into PreemptModels
        /// </summary>
        /// <param name="events">The input list of BaseEventLogModels</param>
        /// <param name="eventCode">The event code to filter by</param>
        /// <returns>A filtered list of BaseEventLogModels</returns>

        public static async Task<ConcurrentBag<BaseEventLogModel>> FilterByEventCode(
            ConcurrentBag<BaseEventLogModel> events, long eventCode)
        {
            try
            {
                return new ConcurrentBag<BaseEventLogModel>( events.Where(x => x.EventCode == eventCode));
            }
            catch (Exception e)
            {
                await _logger.WriteToErrorLogAsync(fileName, "FilterByEventCode", e);
                return [];
            }
        }



        /// <summary>
    /// Starts consumer tasks that filter by event codes and process signals from the BlockingCollection.
    /// </summary>
    /// <param name="signalQueue">The BlockingCollection containing signals to process.</param>
    /// <param name="eventCodes">List of event codes to filter signals.</param>
    /// <param name="token">Cancellation token for graceful shutdown.</param>
    /// <returns>A list of consumer tasks.</returns>
    private IEnumerable<Task> StartFilteredConsumers(BlockingCollection<BaseEventLogModel> signalQueue, List<long?> eventCodes, CancellationToken token)
        {
            int consumerCount = Environment.ProcessorCount; // Adjust based on your needs
            var consumers = new List<Task>();

            for (int i = 0; i < consumerCount; i++)
            {
                var consumerTask = Task.Run(async () =>
                {
                    foreach (var signal in signalQueue.GetConsumingEnumerable(token))
                    {
                        try
                        {
                            // Filter by event code
                            if (eventCodes.Contains(signal.EventCode))
                            {
                                SignalEvents.Add(signal);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing signal: {ex.Message}");
                            await _logger.WriteToErrorLogAsync(fileName, "StartFilteredConsumers", ex, LogLevel.Error);
                            // Optionally, log the error or handle it as needed
                        }
                    }
                }, token);

                consumers.Add(consumerTask);
            }

            return consumers;
        }

        #endregion Helper Methods

        #region Signal Processing

        /// <summary>
        /// A method that takes a list of valid dates and a list of signal Ids and event codes and return a list of events that can be used to write to the database chosen
        /// </summary>
        /// <param name="date">The date the operation is being performed on</param>
        /// <param name="signalIdList">A list of signal Ids to be retrieved</param>
        /// <param name="eventCodes"></param>
        /// <returns>A List of signals that can be used to write to the flash event server</returns>
        /// <exception cref="ArgumentException">Thrown when event codes are used without signalIDs</exception>
        public async Task<bool> ProcessEvents(DateTime date, List<long?>? signalIdList, List<long?>? eventCodes)
        {

            if (eventCodes == null || eventCodes.Count == 0)
                throw new ArgumentException("EventCodes cannot be null or empty.");

            const int boundedCapacity = 1000;

            using var semaphore = new SemaphoreSlim(ThreadCount, maxCount: ThreadCount);
            using var signalQueue = new BlockingCollection<BaseEventLogModel>(boundedCapacity);
            var cts = new CancellationTokenSource();
            var token = cts.Token;

            var consumerTasks = StartFilteredConsumers(signalQueue, eventCodes, token);
            try
            {
                using var client = new AmazonS3Client(AwsAccess, AwsSecret, BucketRegion);

                var s3Objects = await GetListRequest(client, date, signalIdList);

                if (s3Objects is not { Count: not 0 })
                {
                    Console.WriteLine($"No S3 objects found for date {date.ToShortDateString()}");
                    return false;
                }
                var producerTasks = s3Objects.Select(obj => ProcessS3ObjectAsync(obj, client, signalQueue, semaphore, token)).ToList();

                // Await all producer tasks for the current date
                await Task.WhenAll(producerTasks);
                Console.WriteLine(date);
                signalQueue.CompleteAdding();
                await Task.WhenAll(consumerTasks);
                Console.WriteLine($"Processing {date.Day} completed successfully");

                return true;
            }

            catch (Exception e)
            {
                await _logger.WriteToErrorLogAsync(fileName, "ProcessEvents", e, LogLevel.Error);
                await cts.CancelAsync();
                return false;

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="validDates"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public async Task<List<DateTime>> GetEventLogsAsync(List<DateTime> validDates, string TableName)
        {

            await StateSwitcherAsync();

            var query = $"""
                         
                             SELECT *
                             FROM `{TableName}`
                             WHERE `Timestamp` BETWEEN @StartDate AND @EndDate
                         """;

            await using var command = new MySqlCommand(query, MySqlConnection);
            command.Parameters.Add(new MySqlParameter("@StartDate", MySqlDbType.DateTime)
                { Value = validDates.FirstOrDefault() });
            command.Parameters.Add(new MySqlParameter("@EndDate", MySqlDbType.DateTime)
                { Value = validDates.LastOrDefault() });

            await using var reader = await command.ExecuteReaderAsync();
            if (!reader.HasRows) return validDates;

            var datesToProcess = new HashSet<DateTime>(validDates.Select(d => d.Date)); 
            while (await reader.ReadAsync())
            {
                var log = new BaseEventLogModel
                {
                    Timestamp = reader.GetDateTime("Timestamp"),
                    SignalID = reader.IsDBNull("SignalID") ? (long?)null : reader.GetInt64("SignalID"),
                    EventCode = reader.IsDBNull("EventCode") ? (long?)null : reader.GetInt64("EventCode"),
                    EventParam = reader.IsDBNull("EventParam") ? (long?)null : reader.GetInt64("EventParam")
                };

                SignalEvents.Add(log);
                datesToProcess.Remove(log.Timestamp.Date);
            }

            return datesToProcess.ToList();
        }


        /// <summary>
        /// Processes a single S3 object: fetches, deserializes, and enqueues signals.
        /// </summary>
        /// <param name="obj">The S3 object to process.</param>
        /// <param name="client">AmazonS3Client instance.</param>
        /// <param name="signalQueue">BlockingCollection to enqueue signals.</param>
        /// <param name="semaphore">SemaphoreSlim to control concurrency.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private static async Task ProcessS3ObjectAsync(
            S3Object obj,
            AmazonS3Client client,
            BlockingCollection<BaseEventLogModel> signalQueue,
            SemaphoreSlim semaphore,
            CancellationToken token)
        {
            await semaphore.WaitAsync(token);

            try
            {
                // Fetch the S3 object
                using var response = await MemoryStreamHelper(obj, client);
                using var ms = new MemoryStream();
                await response.ResponseStream.CopyToAsync(ms, 81920, token); // 80KB buffer

                // Reset position before deserialization
                ms.Position = 0; 

                // Deserialize Parquet data
                var signalData = await ParquetConvert.DeserializeAsync<BaseEventLogModel>(ms, cancellationToken: token);

                // Enqueue all deserialized signals
                foreach (var signal in signalData)
                {
                    signalQueue.Add(signal, token);
                }
            }
            catch (ArgumentException ex) when (ex.ParamName == "destination")
            {
                Console.WriteLine($"ArgumentException: {ex.Message}, Object Key: {obj.Key}");
                await _logger.WriteToErrorLogAsync(fileName, "ProcessS3ObjectAsync", ex, LogLevel.Error);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"FormatException: {ex.Message}, Object Key: {obj.Key}");
                await _logger.WriteToErrorLogAsync(fileName, "ProcessS3ObjectAsync", ex, LogLevel.Error);
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"Processing canceled for object: {obj.Key}");
                await _logger.WriteToErrorLogAsync(fileName, "ProcessS3ObjectAsync", ex, LogLevel.Error);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected exception: {ex.Message}, Object Key: {obj.Key}");
                await _logger.WriteToErrorLogAsync(fileName, "ProcessS3ObjectAsync", ex, LogLevel.Error);
            }
            finally
            {
                semaphore.Release();
            }
        }


        #endregion Signal Processing

    }
}