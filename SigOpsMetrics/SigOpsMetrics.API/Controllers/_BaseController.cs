using System;
using System.Data;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SigOpsMetrics.API.Classes;
#pragma warning disable 1591

namespace SigOpsMetrics.API.Controllers
{
    public class _BaseController : ControllerBase
    {
        internal static readonly RegionEndpoint BucketRegion = RegionEndpoint.USEast1;
        internal AppConfig AppConfig;
        internal MySqlConnection SqlConnectionReader;
        internal MySqlConnection SqlConnectionWriter;

        public _BaseController(IOptions<AppConfig> settings, IConfiguration configuration)
        {
            AppConfig = settings.Value;
            SqlConnectionReader = new MySqlConnection(configuration.GetConnectionString("Reader"));
            SqlConnectionWriter = new MySqlConnection(configuration.GetConnectionString("Writer"));
        }

        internal IAmazonS3 CreateS3Client()
        {
            return new AmazonS3Client(AppConfig.AWSAccessKey, AppConfig.AWSSecretKey, BucketRegion);
        }


    }
}
