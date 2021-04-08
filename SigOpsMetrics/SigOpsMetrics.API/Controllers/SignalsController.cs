using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.Extensions.Caching.Memory;
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
    [Route("signals")]
    public class SignalsController : _BaseController
    {
        private const string KeyName = "Corridors_Latest.xlsx";
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="connection"></param>
        /// <param name="cache"></param>
        public SignalsController(IOptions<AppConfig> settings, MySqlConnection connection, IMemoryCache cache) : base(settings, connection, cache)
        {
        }

        #region Endpoints

        /// <summary>
        /// Return a list of signals + all details in the SigOps system
        /// </summary>
        /// <returns></returns>
        //todo: known issue with Swagger - this crashes it due to response size
        //todo: Leave it for now and hope Swagger fixes it down the road - MJW 3/2/21
        [HttpGet("all")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<SignalDTO>> Get()
        {
            const string cacheName = "signals/all";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    var worksheet = GetSpreadsheet();

                    var retVal = GetAllSignalData(await worksheet);
                    return retVal;
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of signal names in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("names")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetNames()
        {
            const string cacheName = "signals/names";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    var worksheet = GetSpreadsheet();

                    var retVal = GetSignalNames(await worksheet);
                    return retVal;
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of zone groups in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("zonegroups")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetZoneGroups()
        {
            const string cacheName = "signals/zonegroups";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    var worksheet = GetSpreadsheet();

                    var retVal = GetZoneGroups(await worksheet);
                    return retVal;

                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of zones in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("zones")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetZones()
        {
            const string cacheName = "signals/zones";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    var worksheet = GetSpreadsheet();

                    var retVal = GetZones(await worksheet);
                    return retVal;
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        [HttpGet("zonesbyzonegroup")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetZonesByZoneGroup(string zoneGroup)
        {
            string cacheName = $"signals/zonesbyzonegroup/{zoneGroup}";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;
                    var worksheet = GetSpreadsheet();

                    var retVal = GetZonesByZoneGroup(await worksheet, zoneGroup);

                    return retVal;
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of corridors in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("corridors")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetCorridors()
        {
            const string cacheName = "signals/corridors";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    var worksheet = GetSpreadsheet();

                    var retVal = GetCorridors(await worksheet);
                    return retVal;
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of corridors by zone in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("corridorsbyzone")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetCorridorsByZone(string zone)
        {
            var cacheName = $"signals/corridorsbyzone/{zone}";
            try
            {
                
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    var worksheet = GetSpreadsheet();

                    var retVal = GetCorridorsByZone(await worksheet, zone);
                    return retVal;
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        [HttpGet("corridorsbyzonegroup")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetCorridorsByZoneGroup(string zoneGroup)
        {
            var cacheName = $"signals/corridorsbyzonegroup/{zoneGroup}";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    var worksheet = GetSpreadsheet();
                    var retVal = GetCorridorsByZoneGroup(await worksheet, zoneGroup);
                    return retVal;
                });

                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of subcorridors in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("subcorridors")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetSubCorridors()
        {
            const string cacheName = "signals/subcorridors";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    var worksheet = GetSpreadsheet();

                    var retVal = GetSubCorridors(await worksheet);
                    return retVal;
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of subcorridors by corridor in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("subcorridorsbycorridor")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetSubCorridorsByCorridor(string corridor)
        {
            var cacheName = $"signals/subcorridorsbycorridor/{corridor}";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    var worksheet = GetSpreadsheet();

                    var retVal = GetSubCorridorsByCorridor(await worksheet, corridor);
                    return retVal;
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        [HttpGet("agencies")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetAgencies(string corridor)
        {
            const string cacheName = "signals/agencies";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    var worksheet = GetSpreadsheet();
                    var retVal = GetAgencies(await worksheet);
                    return retVal;
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
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
                        Note = sheet.Cells[row, ++col].Text,
                        Latitude = sheet.Cells[row, ++col].Text.ToDouble(),
                        Longitude = sheet.Cells[row, ++col].Text.ToDouble()
                    };
                    retVal.Add(newSignal);
                }

                return retVal.Where(x => x.SignalID != "-1");
            }
            catch (Exception ex)
            {
                DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/getallsignaldata", ex).GetAwaiter();
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
                DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/getsignalnames", ex).GetAwaiter();
            }

            return new List<string>();
        }

        private IEnumerable<string> GetZoneGroups(ExcelWorksheet sheet)
        {
            var retVal = GetSingleColumnFromSpreadsheet(sheet, 2).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()
                .OrderBy(x => x).ToList();
            //retVal.Insert(0, "All RTOP"); //Hardcode in an 'all' option - will have to check for special case in Metrics controller
            return retVal;
        }

        private IEnumerable<string> GetZones(ExcelWorksheet sheet)
        {
            return GetSingleColumnFromSpreadsheet(sheet, 3).Distinct().OrderBy(x => x);
        }

        private IEnumerable<string> GetZonesByZoneGroup(ExcelWorksheet sheet, string zoneGroupName)
        {
            try
            {
                var start = sheet.Dimension.Start;
                var end = sheet.Dimension.End;

                var retVal = new List<string>();

                for (var row = start.Row + 1; row <= end.Row; row++)
                {
                    if (string.Equals(sheet.Cells[row, 2].Text.Trim(), zoneGroupName,
                        StringComparison.CurrentCultureIgnoreCase))
                        retVal.Add(sheet.Cells[row, 3].Text.Trim());
                }

                return retVal.Distinct();
            }
            catch (Exception ex)
            {
                DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/getzonesbyzonegroup", ex).GetAwaiter();
            }

            return new List<string>();
        }

        private IEnumerable<string> GetCorridorsByZoneGroup(ExcelWorksheet sheet, string zoneGroupName)
        {
            try
            {
                var start = sheet.Dimension.Start;
                var end = sheet.Dimension.End;

                var retVal = new List<string>();

                for (var row = start.Row + 1; row <= end.Row; row++)
                {
                    if (string.Equals(sheet.Cells[row, 2].Text.Trim(), zoneGroupName,
                        StringComparison.CurrentCultureIgnoreCase))
                        retVal.Add(sheet.Cells[row, 4].Text.Trim());
                }

                return retVal.Distinct();
            }
            catch(Exception ex)
            {
                DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/getcorridorsbyzonegroup", ex).GetAwaiter();
            }

            return new List<string>();
        }

        private IEnumerable<string> GetCorridors(ExcelWorksheet sheet)
        {
            return GetSingleColumnFromSpreadsheet(sheet, 4).Distinct().OrderBy(x => x);
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
                DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/getcorridorsbyzone", ex).GetAwaiter();
            }

            return new List<string>();
        }

        private IEnumerable<string> GetSubCorridors(ExcelWorksheet sheet)
        {
            return GetSingleColumnFromSpreadsheet(sheet, 5).Distinct().OrderBy(x => x);
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
                DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/getsubcorridorsbycorridor", ex).GetAwaiter();
            }

            return new List<string>();
        }

        private IEnumerable<string> GetAgencies(ExcelWorksheet sheet)
        {
            return GetSingleColumnFromSpreadsheet(sheet, 6).Distinct().OrderBy(x => x);
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
                    var stringToEnter = sheet.Cells[row, col].Text.Trim();
                    if (!string.IsNullOrWhiteSpace(stringToEnter))
                        retVal.Add(stringToEnter);
                }

                return retVal;
            }
            catch (Exception ex)
            {
                DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    $"signals/getsinglecolumnfromspreadsheet/{col}", ex).GetAwaiter();
            }

            return new List<string>();
        }

        

        #endregion
        
    }
}
