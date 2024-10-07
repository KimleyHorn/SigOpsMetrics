using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Parquet;
using SigOpsMetricsCalcEngine.Models;


public class DataAccessLayer
{
    private readonly string AwsAccess;
    private readonly string AwsSecret;
    private readonly RegionEndpoint BucketRegion;
    private readonly string AwsBucketName;
    private readonly int ThreadCount; // Number of concurrent producer tasks

    public DataAccessLayer(string awsAccess, string awsSecret, RegionEndpoint bucketRegion, string awsBucketName, int threadCount = 4)
    {
        AwsAccess = awsAccess;
        AwsSecret = awsSecret;
        BucketRegion = bucketRegion;
        AwsBucketName = awsBucketName;
        ThreadCount = threadCount;
    }

    /// <summary>
    /// Processes events by fetching Parquet files from S3, deserializing them, and filtering by event codes.
    /// Utilizes BlockingCollection to manage memory usage effectively.
    /// </summary>
    /// <param name="validDates">List of valid dates to process.</param>
    /// <param name="eventCodes">List of event codes to filter by.</param>
    /// <returns>True if processing succeeds; otherwise, false.</returns>
    public async Task<bool> ProcessEvents(
        List<DateTime> validDates,
        List<long?> eventCodes)
    {
        if (eventCodes == null || eventCodes.Count == 0)
            throw new ArgumentException("EventCodes cannot be null or empty.");

        // Define the bounded capacity based on your system's memory and expected load
        int boundedCapacity = 1000;

        // Initialize BlockingCollection with bounded capacity
        using var signalQueue = new BlockingCollection<BaseEventLogModel>(boundedCapacity);

        // Initialize SemaphoreSlim to control the number of concurrent producer tasks
        using var semaphore = new SemaphoreSlim(ThreadCount, ThreadCount);

        // Initialize CancellationTokenSource for graceful shutdown (optional)
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Start consumer tasks that will filter by event codes and process signals
        var consumerTasks = StartFilteredConsumers(signalQueue, eventCodes, token);

        try
        {
            using var client = new AmazonS3Client(AwsAccess, AwsSecret, BucketRegion);

            foreach (var date in validDates)
            {
                ConcurrentBag<S3Object> s3Objects = await GetListRequest(client, date);

                if (s3Objects is not { Count: not 0 })
                {
                    Console.WriteLine($"No S3 objects found for date {date.ToShortDateString()}");
                    continue;
                }

                var producerTasks = s3Objects.Select(obj => ProcessS3ObjectAsync(obj, client, signalQueue, semaphore, token)).ToList();

                // Await all producer tasks for the current date
                await Task.WhenAll(producerTasks);
            }

            // Signal that no more items will be added
            signalQueue.CompleteAdding();

            // Wait for all consumer tasks to finish processing
            await Task.WhenAll(consumerTasks);

            Console.WriteLine("Processing completed successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            // Optionally, cancel consumer tasks
            cts.Cancel();
            return false;
        }
    }

    /// <summary>
    /// Starts consumer tasks that filter by event codes and process signals from the BlockingCollection.
    /// </summary>
    /// <param name="signalQueue">The BlockingCollection containing signals to process.</param>
    /// <param name="eventCodes">List of event codes to filter signals.</param>
    /// <param name="token">Cancellation token for graceful shutdown.</param>
    /// <returns>A list of consumer tasks.</returns>
    private List<Task> StartFilteredConsumers(BlockingCollection<BaseEventLogModel> signalQueue, List<long?> eventCodes, CancellationToken token)
    {
        int consumerCount = Environment.ProcessorCount; // Adjust based on your needs
        var consumers = new List<Task>();

        for (int i = 0; i < consumerCount; i++)
        {
            var consumerTask = Task.Run(() =>
            {
                foreach (var signal in signalQueue.GetConsumingEnumerable(token))
                {
                    try
                    {
                        // Filter by event code
                        if (eventCodes.Contains(signal.EventCode))
                        {
                            ProcessSignal(signal); // Process only if event code matches
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing signal: {ex.Message}");
                        // Optionally, log the error or handle it as needed
                    }
                }
            }, token);

            consumers.Add(consumerTask);
        }

        return consumers;
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
    private async Task ProcessS3ObjectAsync(
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

            ms.Position = 0; // Reset position before deserialization

            // Deserialize Parquet data
            var signalData = await ParquetConvert.DeserializeAsync<BaseEventLogModel>(ms);

            // Enqueue all deserialized signals
            foreach (var signal in signalData)
            {
                // This will block if the collection is full, providing backpressure
                signalQueue.Add(signal, token);
            }
        }
        catch (ArgumentException ex) when (ex.ParamName == "destination")
        {
            Console.WriteLine($"ArgumentException: {ex.Message}, Object Key: {obj.Key}");
            // Optionally, log the exception or handle it as needed
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"FormatException: {ex.Message}, Object Key: {obj.Key}");
            // Optionally, log the exception or handle it as needed
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation if needed
            Console.WriteLine($"Processing canceled for object: {obj.Key}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected exception: {ex.Message}, Object Key: {obj.Key}");
            // Optionally, log the exception or handle it as needed
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Processes a single signal. Implement your processing logic here.
    /// </summary>
    /// <param name="signal">The signal to process.</param>
    private void ProcessSignal(BaseEventLogModel signal)
    {
        // Implement your processing logic here.
        // For example, write to a database, send to another service, etc.
        Console.WriteLine($"Processed Signal: EventCode = {signal.EventCode}");
    }

    /// <summary>
    /// A helper method that retrieves an S3 object from the specified bucket.
    /// </summary>
    /// <param name="obj">The S3 object to retrieve.</param>
    /// <param name="client">The AmazonS3Client instance.</param>
    /// <returns>A GetObjectResponse containing the S3 object data.</returns>
    private static async Task<GetObjectResponse> MemoryStreamHelper(S3Object obj, AmazonS3Client client)
    {
        var request = new GetObjectRequest
        {
            BucketName = obj.BucketName ?? throw new ArgumentNullException(nameof(obj.BucketName)),
            Key = obj.Key
        };
        Console.WriteLine($"Requested object: {obj.Key} from bucket {obj.BucketName}");
        return await client.GetObjectAsync(request);
    }

    /// <summary>
    /// Retrieves a list of S3 objects based on the specified date.
    /// </summary>
    /// <param name="client">The AmazonS3Client instance.</param>
    /// <param name="date">The date to filter S3 objects.</param>
    /// <returns>A ConcurrentBag containing the filtered S3 objects.</returns>
    private async Task<ConcurrentBag<S3Object>> GetListRequest(AmazonS3Client client, DateTime date)
    {
        // Implement your logic to list S3 objects based on date.
        // This example assumes objects are stored under a prefix based on date.

        var prefix = $"logs/{date:yyyy/MM/dd}/"; // Adjust based on your S3 key structure

        var request = new ListObjectsV2Request
        {
            BucketName = AwsBucketName,
            Prefix = prefix
        };

        var response = await client.ListObjectsV2Async(request);
        var s3Objects = new ConcurrentBag<S3Object>(response.S3Objects.Where(o => o.Size > 0));

        return s3Objects;
    }
}
