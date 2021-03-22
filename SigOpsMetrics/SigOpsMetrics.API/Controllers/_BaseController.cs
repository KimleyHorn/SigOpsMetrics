using System;
using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
        internal MySqlConnection SqlConnection;
        internal readonly IMemoryCache Cache;

        internal TimeSpan OneHourCache = TimeSpan.FromHours(1);
        internal TimeSpan SixHourCache = TimeSpan.FromHours(6);

        public _BaseController(IOptions<AppConfig> settings, MySqlConnection connection, IMemoryCache cache)
        {
            AppConfig = settings.Value;
            SqlConnection = connection;
            Cache = cache;
        }

        internal IAmazonS3 CreateS3Client()
        {
            return new AmazonS3Client(AppConfig.AWSAccessKey, AppConfig.AWSSecretKey, BucketRegion);
        }
    }
}
