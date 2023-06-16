using System.Configuration;
using System.Data;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Google.Api.Gax.ResourceNames;
using MySqlConnector;
using Parquet;
using SigOpsMetricsCalcEngine.Data;
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
        internal static readonly string? MySqlDbName = ConfigurationManager.AppSettings["DB_NAME"];
        internal static readonly string? MySqlConnString = ConfigurationManager.AppSettings["CONN_STRING"];
        internal static MySqlConnection MySqlConnection;

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

        internal static async Task<bool> MySqlHelper(string MySqlTableName, DataTable dataTable)
        {
            //Write a conditional statement that returns true if the data was written to the table successfully
            MySqlConnection.Open();
            Console.WriteLine("Connection Opened");
            var bulkCopy = new MySqlBulkCopy(MySqlConnection)
            {
                DestinationTableName = $"{MySqlDbName}.{MySqlTableName}"
            };
            Console.WriteLine("Bulk Copy Created");
            await bulkCopy.WriteToServerAsync(dataTable);
            Console.WriteLine("Bulk Copy Written");
            await MySqlConnection.CloseAsync();
            Console.WriteLine("Connection Closed");
            Console.WriteLine("Flash Events written to Database");
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
