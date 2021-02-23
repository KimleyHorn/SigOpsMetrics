using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.DTOs;
using SigOpsMetrics.API.Classes.Extensions;

namespace SigOpsMetrics.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignalsController : _BaseController
    {
        private const string KeyName = "Corridors_Latest.xlsx";

        public SignalsController(IOptions<AppConfig> settings) : base(settings)
        {
        }

        [HttpGet]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<SignalDTO>> Get()
        {
            try
            {
                var client = CreateS3Client();

                var request = new GetObjectRequest
                {
                    BucketName = AppConfig.AWSBucketName,
                    Key = KeyName
                };

                var ms = new MemoryStream();

                using (var getObjectResponse = await client.GetObjectAsync(request))
                {
                    await getObjectResponse.ResponseStream.CopyToAsync(ms);
                }

                var package = new ExcelPackage(ms);
                var worksheet = package.Workbook.Worksheets[0];

                var retVal = GetSignalData(worksheet);
                return retVal;

            }
            catch (Exception ex)
            {
                var e = ex;
                return null;
            }
        }

        private IEnumerable<SignalDTO> GetSignalData(ExcelWorksheet sheet)
        {
            var start = sheet.Dimension.Start;
            var end = sheet.Dimension.End;

            var retVal = new List<SignalDTO>();

            //todo error handling
            //Headers in row 1, data starts in row 2
            for (var row = start.Row + 1; row <= end.Row; row++)
            {
                //Excel is 1-based
                var col = 1;
                var newSignal = new SignalDTO
                {
                    SignalID = sheet.Cells[row, col].Text,
                    ZoneGroup = sheet.Cells[row, ++col].Text,
                    Zone = sheet.Cells[row, ++col].Text,
                    Corridor = sheet.Cells[row, ++col].Text,
                    Subcorridor = sheet.Cells[row, ++col].Text,
                    Agency = sheet.Cells[row, ++col].Text,
                    MainStreetName = sheet.Cells[row, ++col].Text,
                    SideStreetName = sheet.Cells[row, ++col].Text,
                    Milepost = sheet.Cells[row, ++col].Text,
                    AsOf = sheet.Cells[row, ++col].Text.ToNullableDateTime(),
                    Duplicate = sheet.Cells[row, ++col].Text,
                    Include = sheet.Cells[row, ++col].Text,
                    Modified = sheet.Cells[row, ++col].Text.ToNullableDateTime(),
                    Note = sheet.Cells[row, ++col].Text
                };
                retVal.Add(newSignal);
            }

            return retVal;
        }
    }
}
