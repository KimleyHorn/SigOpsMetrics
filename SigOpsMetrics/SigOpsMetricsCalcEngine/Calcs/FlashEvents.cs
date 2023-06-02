using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
//using System.IO.MemoryMappedFiles.MemoryMappedViewStream;
using SigOpsMetrics.API;
using SigOpsMetricsCalcEngine.Classes;
using MySqlConnector;

namespace SigOpsMetricsCalcEngine.Calcs
{
    public class FlashEvents
    {
        private readonly BasicAWSCredentials _awsCredentials;
        //private readonly AmazonS3Client _s3Client;
        private readonly RegionEndpoint _regionEndpoint;

        //TODO: Reference the appsettings.json file

        //TODO: Start testing the aws connection
        //TODO: Refactor code to mimic SigOpsMetrics.API
        //TODO: Flesh out flasheveventsdataaccesslayer class
        //TODO: implement flasheventdataaccesslayer class into flashevents class


        //private readonly string _bucketName = "your-bucket-name";
        //private readonly string _keyName = "your-key-name";

        public List<FlashEvent> FlashList { get ; set ; }



        /// <summary>
        /// Constructor for the FlashEvents class with params for the aws credentials and region.
        /// This class will be used to get the file from aws, read the file, find the flash events, and calculate how long each flash event lasted.
        /// </summary>
        /// <param name="flashEvents"></param>
        /// <param name="awsCredentials"></param>
        /// <param name="region"></param>
        //Constructor
        public FlashEvents(BasicAWSCredentials awsCredentials, RegionEndpoint region, List<FlashEvent> flashEvents)
        {
            _awsCredentials = awsCredentials;

            _regionEndpoint = region;
            FlashList = flashEvents;

        }

        //Get the file from aws

        private async Task<GetObjectResponse> GetFileAsync(string bucketName, string keyName)
        {
            // Set RegionEndpoint to your AWS region.
            AmazonS3Config config = new()
            {
                RegionEndpoint = _regionEndpoint
            };

            using var client = new AmazonS3Client(_awsCredentials, config);
            return await client.GetObjectAsync(bucketName, keyName);
        }

        //Research how to download the file from aws through memory stream

        public async Task<byte[]> DownloadFileFromS3Async(string bucketName, string keyName)
        {
            var response = await GetFileAsync(bucketName, keyName);
            var fileBytes = new byte[response.ContentLength];
            await response.ResponseStream.ReadAsync(fileBytes, 0, (int)response.ContentLength);
            return fileBytes;
        }

        //Research how to read the file from memory stream

        //read a file from memory stream


        //Find the flash events in this file


        //Calculate how long each flash event lasted
        //Save memory stream to mysql

        private static void SaveFlashEventsToMySql(byte[] flashEventBytes)
        {
            using var writer = new StringWriter();
            writer.Write(flashEventBytes);

            //connect to mysql server
            MySqlConnection m = new();
            m.Open();

            m.Close();

            //create a table for the flash events
            writer.Flush();
            writer.Close();


            //Save the flash events to mysql
        }


    }
}
