namespace SigOpsMetrics.API.Classes
{
    public class AppConfig
    {
        public string AWSAccessKey { get; set; }
        public string AWSSecretKey { get;set; }
        public string AWSBucketName { get; set; }
        public string DataPullKey { get; set; }
        public string CorridorsKey { get; set; }
        public string CamerasKey { get; set; }
        public static string SmtpUsername { get; set; }
        public static string SmtpPassword { get; set; }
        public static string DatabaseName { get; set; }
        public static string GoogleBucketName { get; set; }
        public static string GoogleFolderName { get; set; }
    }
}
