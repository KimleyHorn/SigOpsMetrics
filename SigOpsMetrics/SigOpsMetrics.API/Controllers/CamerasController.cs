using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;
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
        /// Endpoint for performing daily data pull of cameras_latest.xls into sql table.
        /// </summary>
        /// <returns></returns>
        [HttpGet("datapull/{key}")]
        public async Task<IActionResult> DataPull(string key)
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

                var worksheet = GetSpreadsheet();
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

        private async Task<ExcelWorksheet> GetSpreadsheet()
        {
            var client = CreateS3Client();

            var request = new GetObjectRequest
            {
                BucketName = AppConfig.AWSBucketName,
                Key = AppConfig.CamerasKey
            };

            var ms = new MemoryStream();

            using (var getObjectResponse = await client.GetObjectAsync(request))
            {
                await getObjectResponse.ResponseStream.CopyToAsync(ms);
            }

            var package = new ExcelPackage(ms);
            return package.Workbook.Worksheets[0];
        }

        #endregion
    }
}
