using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SigOpsMetrics.API.Classes;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using SigOpsMetrics.API.DataAccess;

namespace SigOpsMetrics.API.Controllers
{
    /// <summary>
    /// Controller for CCTV data
    /// </summary>
    [ApiController]
    [Route("cameras")]
    public class CamerasController : _BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="configuration"></param>
        public CamerasController(IOptions<AppConfig> settings, IConfiguration configuration) : base(settings, configuration)
        {
        }

        #region Endpoints

        /// <summary>
        /// Endpoint for performing daily data pull of cameras_latest.xls into sql table.  Destination 0 = S3, 1 = GCP
        /// </summary>
        /// <returns></returns>
        [HttpGet("datapull/{key}/{destination}")]
        public async Task<IActionResult> DataPull(string key, int destination)
        {
            try
            {
                if (key != AppConfig.DataPullKey)
                {
                    await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
            System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name,
                "DataPull", new Exception($"Invalid Key: {key}"));
                    return BadRequest("Invalid Key");
                }

                var worksheet = GetSpreadsheet((GenericEnums.DataPullSource)destination);
                var ws = await worksheet;
                await CamerasDataAccessLayer.WriteToCameras(SqlConnectionWriter, ws);
                return Ok();
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name,
                    "DataPull", ex);
                return BadRequest(ex);
            }
        }

        #endregion

        #region Private Methods

        private async Task<ExcelWorksheet> GetSpreadsheet(GenericEnums.DataPullSource destination)
        {
            var ms = new MemoryStream();
            switch (destination)
            {
                case GenericEnums.DataPullSource.S3:
                    {
                        var client = CreateS3Client();

                        var request = new GetObjectRequest
                        {
                            BucketName = AppConfig.AWSBucketName,
                            Key = AppConfig.CamerasKey
                        };

                        using var getObjectResponse = await client.GetObjectAsync(request);
                        await getObjectResponse.ResponseStream.CopyToAsync(ms);
                        break;
                    }
                case GenericEnums.DataPullSource.Google:
                    {
                        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "castle-rock-service-account.json");
                        var storage = await StorageClient.CreateAsync();
                        storage.DownloadObject(AppConfig.GoogleBucketName, AppConfig.GoogleFolderName + "/" + AppConfig.CamerasKey, ms);
                        break;
                    }
            }

            var package = new ExcelPackage(ms);
            return package.Workbook.Worksheets[0];
        }

        #endregion
    }
}
