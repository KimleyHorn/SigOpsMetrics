using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using MySqlConnector;
using Parquet;
using System.Configuration;
using Amazon.Runtime.Internal;
using Parquet.Thrift;
using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    public class FlashEventCalc
    {

        private static readonly string? AWSAccess = ConfigurationManager.AppSettings["S3_ACCESS_KEY"];
        private static readonly string? AWSsecret = ConfigurationManager.AppSettings["S3_SECRET_KEY"];
        private static readonly string? AWSBucketName = ConfigurationManager.AppSettings["S3_BUCKET_NAME"];
        private static readonly RegionEndpoint? BucketRegion = RegionEndpoint.USEast1;
        private static readonly string? FolderName = ConfigurationManager.AppSettings["FOLDER_NAME"];
        private static readonly int ThreadCount = int.Parse(ConfigurationManager.AppSettings["THREAD_COUNT"] ?? "1");

        FlashEventCalc()
        {

        }

        /// <summary>
        /// This method will return a list of events from AWS S3 and fit them to the FlashEventModel class for use in the rest of the solution
        /// </summary>
        /// <param name="dateRange"></param>
        /// <returns>A List of Flash events that can be used to write to the flash_event_log server</returns>
        public static async Task ProcessFlashEvents(DateTime startDate, DateTime endDate = default(DateTime))
        {
            //TODO: make parameters start and end dates and iterate through all dates until end date
            if (endDate == default(DateTime))
            {
                endDate = startDate;
            }

            if (startDate > endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            try
            {
                var events = new List<FlashEventModel>();
                using var client = new AmazonS3Client(AWSAccess, AWSsecret, BucketRegion);
                while(startDate <= endDate)
                {

                    var listRequest = new ListObjectsV2Request
                    { 
                        BucketName = AWSBucketName,
                        Prefix = FolderName + "/date=" + startDate.ToString("yyyy-MM-dd")
                    };

                    var res = await client.ListObjectsV2Async(listRequest);

                    var s3Objects = res.S3Objects.ToList(); // Convert the collection to a list

                    
                    var semaphore = new SemaphoreSlim(ThreadCount);

                    //For debugging purposes to speed up data processing
                    //                    #if DEBUG
                    //                    var elementToKeep = s3Objects[0]; // Choose the element you want to keep

                    //                    s3Objects.RemoveAll(obj => obj != elementToKeep);

                    //                    #endif
                    var tasks = s3Objects.Select(async obj =>
                    {
                        await semaphore.WaitAsync();

                        try
                        {
                            var request = new GetObjectRequest { 
                                BucketName = AWSBucketName,
                                Key = obj.Key
                            };
                            Console.WriteLine(obj.Key);
                            var response = await client.GetObjectAsync(request);
                            using var ms = new MemoryStream();
                            await response.ResponseStream.CopyToAsync(ms);
                            var data = await ParquetConvert.DeserializeAsync<FlashEventModel>(ms);
                            return data.Where(x => x.EventCode == 173).ToList();
                        }
                        finally { semaphore.Release(); }
                    });

                    var results = await Task.WhenAll(tasks);

                    foreach (var flashList in results)
                    {
                        events.AddRange(flashList);
                    }
                    startDate = startDate.AddDays(1);
                } 
                await FlashEventDataAccessLayer.WriteFlashEventsToDb(events);
            }

            #region Error Handling
            catch (HttpRequestException e)
            {
                Console.WriteLine("HTTP Error" + e.ToString());
                await FlashEventDataAccessLayer.WriteToErrorLog(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name, "toMySQL", e);
                throw;
            }
            catch (HttpErrorResponseException e)
            {
                Console.WriteLine("Wrong Password foo" + e.ToString());
                await FlashEventDataAccessLayer.WriteToErrorLog(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name, "toMySQL", e);
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e.ToString());
                await FlashEventDataAccessLayer.WriteToErrorLog(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name, "toMySQL", e);
                throw;
            }
            #endregion
        }
    }
}
