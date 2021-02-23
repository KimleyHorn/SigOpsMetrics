using Amazon;
using Amazon.S3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SigOpsMetrics.API.Classes;

namespace SigOpsMetrics.API.Controllers
{
    public class _BaseController : ControllerBase
    {
        internal static readonly RegionEndpoint BucketRegion = RegionEndpoint.USEast1;
        internal AppConfig AppConfig;

        public _BaseController(IOptions<AppConfig> settings)
        {
            AppConfig = settings.Value;
        }

        internal IAmazonS3 CreateS3Client()
        {
            return new AmazonS3Client(AppConfig.AWSAccessKey, AppConfig.AWSSecretKey, BucketRegion);
        }
    }
}
