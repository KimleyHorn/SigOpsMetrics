using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using MySqlConnector;
using OfficeOpenXml;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.DTOs;
using SigOpsMetrics.API.Classes.Extensions;

namespace SigOpsMetrics.API.Controllers
{
    /// <summary>
    /// Controller for signals, corridors, and groups
    /// </summary>
    [ApiController]
    [Route("Signals")]
    public class SignalsController : _BaseController
    {
        private const string KeyName = "Corridors_Latest.xlsx";

        public SignalsController(IOptions<AppConfig> settings, MySqlConnection connection) : base(settings, connection)
        {
        }

        #region Endpoints

        /// <summary>
        /// Return a list of signals + all details in the SigOps system
        /// </summary>
        /// <returns></returns>
        //todo: known issue with Swagger - this crashes it due to response size
        //todo: Leave it for now and hope Swagger fixes it down the road - MJW 3/2/21
        [HttpGet("All")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<SignalDTO>> Get()
        {
            try
            {
                var worksheet = GetSpreadsheet();

                var retVal = GetAllSignalData(await worksheet);
                return retVal;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Return a list of signal names in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("Names")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetNames()
        {
            var worksheet = GetSpreadsheet();

            var retVal = GetSignalNames(await worksheet);
            return retVal;
        }

        /// <summary>
        /// Return a list of zone groups in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("ZoneGroups")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetZoneGroups()
        {
            var worksheet = GetSpreadsheet();

            var retVal = GetZoneGroups(await worksheet);
            return retVal;
        }

        /// <summary>
        /// Return a list of zones in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("Zones")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetZones()
        {
            var worksheet = GetSpreadsheet();

            var retVal = GetZones(await worksheet);
            return retVal;
        }

        /// <summary>
        /// Return a list of corridors in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("Corridors")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetCorridors()
        {
            var worksheet = GetSpreadsheet();

            var retVal = GetCorridors(await worksheet);
            return retVal;
        }

        /// <summary>
        /// Return a list of corridors by zone in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("CorridorsByZone")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetCorridorsByZone(string zone)
        {
            var worksheet = GetSpreadsheet();

            var retVal = GetCorridorsByZone(await worksheet, zone);
            return retVal;
        }

        /// <summary>
        /// Return a list of subcorridors in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("SubCorridors")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetSubCorridors()
        {
            var worksheet = GetSpreadsheet();

            var retVal = GetSubCorridors(await worksheet);
            return retVal;
        }

        /// <summary>
        /// Return a list of subcorridors by corridor in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("SubCorridorsByCorridor")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetSubCorridorsByCorridor(string corridor)
        {
            var worksheet = GetSpreadsheet();

            var retVal = GetSubCorridorsByCorridor(await worksheet, corridor);
            return retVal;
        }

        #endregion

        #region Private Methods

        private async Task<ExcelWorksheet> GetSpreadsheet()
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
            return package.Workbook.Worksheets[0];
        }

        private IEnumerable<SignalDTO> GetAllSignalData(ExcelWorksheet sheet)
        {
            try
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
            catch (Exception ex)
            {
            }

            return new List<SignalDTO>();
        }

        private IEnumerable<string> GetSignalNames(ExcelWorksheet sheet)
        {
            try
            {
                var start = sheet.Dimension.Start;
                var end = sheet.Dimension.End;

                var retVal = new List<string>();

                for (var row = start.Row + 1; row <= end.Row; row++)
                {
                    var priCol = 7;
                    var secCol = 8;

                    retVal.Add(sheet.Cells[row, priCol].Text.Trim() + " @ " + sheet.Cells[row, secCol].Text.Trim());
                }

                return retVal;
            }
            catch (Exception ex)
            {

            }

            return new List<string>();
        }

        private IEnumerable<string> GetZoneGroups(ExcelWorksheet sheet)
        {
            return GetSingleColumnFromSpreadsheet(sheet, 2).Distinct();
        }

        private IEnumerable<string> GetZones(ExcelWorksheet sheet)
        {
            return GetSingleColumnFromSpreadsheet(sheet, 3).Distinct();
        }

        private IEnumerable<string> GetCorridors(ExcelWorksheet sheet)
        {
            return GetSingleColumnFromSpreadsheet(sheet, 4).Distinct();
        }

        private IEnumerable<string> GetCorridorsByZone(ExcelWorksheet sheet, string zoneName)
        {
            try
            {
                var start = sheet.Dimension.Start;
                var end = sheet.Dimension.End;

                var retVal = new List<string>();

                for (var row = start.Row + 1; row <= end.Row; row++)
                {
                    if (string.Equals(sheet.Cells[row, 3].Text.Trim(), zoneName,
                        StringComparison.CurrentCultureIgnoreCase))
                        retVal.Add(sheet.Cells[row, 4].Text.Trim());
                }

                return retVal.Distinct();
            }
            catch (Exception ex)
            {

            }

            return new List<string>();
        }

        private IEnumerable<string> GetSubCorridors(ExcelWorksheet sheet)
        {
            return GetSingleColumnFromSpreadsheet(sheet, 5).Distinct();
        }

        private IEnumerable<string> GetSubCorridorsByCorridor(ExcelWorksheet sheet, string corridor)
        {
            try
            {
                var start = sheet.Dimension.Start;
                var end = sheet.Dimension.End;

                var retVal = new List<string>();

                for (var row = start.Row + 1; row <= end.Row; row++)
                {
                    if (string.Equals(sheet.Cells[row, 4].Text.Trim(), corridor,
                        StringComparison.CurrentCultureIgnoreCase))
                        retVal.Add(sheet.Cells[row, 5].Text.Trim());
                }

                return retVal.Distinct();
            }
            catch (Exception ex)
            {

            }

            return new List<string>();
        }

        private IEnumerable<string> GetSingleColumnFromSpreadsheet(ExcelWorksheet sheet, int col)
        {
            try
            {
                var start = sheet.Dimension.Start;
                var end = sheet.Dimension.End;

                var retVal = new List<string>();

                for (var row = start.Row + 1; row <= end.Row; row++)
                {
                    retVal.Add(sheet.Cells[row, col].Text.Trim());
                }

                return retVal;
            }
            catch (Exception ex)
            {

            }

            return new List<string>();
        }

        #endregion
        
    }
}
