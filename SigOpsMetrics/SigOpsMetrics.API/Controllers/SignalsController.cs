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
using System.Data;
using SigOpsMetrics.API.DataAccess;

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

                    //var worksheet = GetSpreadsheet();
                    //var retVal = GetAllSignalData(await worksheet);

                    return await SignalsDataAccessLayer.GetAllSignalDataSQL(SqlConnection);
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
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

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetSignalNames(await worksheet);
                    return await SignalsDataAccessLayer.GetSignalNamesSQL(SqlConnection);

                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
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

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetZoneGroups(await worksheet);
                    return await SignalsDataAccessLayer.GetZoneGroupsSQL(SqlConnection);

                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
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

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetZones(await worksheet);
                    return await SignalsDataAccessLayer.GetZonesSQL(SqlConnection);
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        [HttpGet("zonesbyzonegroup/{zoneGroup}")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetZonesByZoneGroup(string zoneGroup)
        {
            string cacheName = $"signals/zonesbyzonegroup/{zoneGroup}";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;
                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetZonesByZoneGroup(await worksheet, zoneGroup);
                    return await SignalsDataAccessLayer.GetZonesByZoneGroupSQL(SqlConnection, zoneGroup);
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
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

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetCorridors(await worksheet);
                    return await SignalsDataAccessLayer.GetCorridorsSQL(SqlConnection);
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of corridors by zone in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("corridorsbyzone/{zone}")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetCorridorsByZone(string zone)
        {
            var cacheName = $"signals/corridorsbyzone/{zone}";
            try
            {
                
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetCorridorsByZone(await worksheet, zone);
                    return await SignalsDataAccessLayer.GetCorridorsByZoneSQL(SqlConnection, zone);
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        [HttpGet("corridorsbyzonegroup/{zoneGroup}")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetCorridorsByZoneGroup(string zoneGroup)
        {
            var cacheName = $"signals/corridorsbyzonegroup/{zoneGroup}";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    //var worksheet = GetSpreadsheet();
                    //var retVal = GetCorridorsByZoneGroup(await worksheet, zoneGroup);
                    return await SignalsDataAccessLayer.GetCorridorsByZoneGroupSQL(SqlConnection, zoneGroup);
                });

                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
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

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetSubCorridors(await worksheet);
                    return await SignalsDataAccessLayer.GetSubCorridorsSQL(SqlConnection);
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of subcorridors by corridor in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("subcorridorsbycorridor/{corridor}")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetSubCorridorsByCorridor(string corridor)
        {
            var cacheName = $"signals/subcorridorsbycorridor/{corridor}";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetSubCorridorsByCorridor(await worksheet, corridor);
                    return await SignalsDataAccessLayer.GetSubCorridorsByCorridorSQL(SqlConnection, corridor);
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        [HttpGet("agencies")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<IEnumerable<string>> GetAgencies()
        {
            const string cacheName = "signals/agencies";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = SixHourCache;

                    //var worksheet = GetSpreadsheet();
                    //var retVal = GetAgencies(await worksheet);
                    return await SignalsDataAccessLayer.GetAgenciesSQL(SqlConnection);
                });
                return await cacheEntry;
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    cacheName, ex);
                return null;
            }
        }

        /// <summary>
        /// Endpoint for performing daily pata pull of corridors_latest.xls into sql table.
        /// </summary>
        /// <returns></returns>
        [HttpGet("datapull/{key}")]
        public async Task DataPull(string key)
        {
            try
            {
                if (key == "45632456236246")
                {
                    Task<ExcelWorksheet> worksheet = GetSpreadsheet();
                    var ws = await worksheet;
                    await SignalsDataAccessLayer.WriteToSignals(SqlConnection, ws);
                }
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "DataPull", ex);
            }
        }

        /// <summary>
        /// Endpoint for performing submitting contact requests.
        /// </summary>
        /// <returns></returns>
        [HttpPost("contact-us")]
        public async Task<IActionResult> ContactUs(ContactInfo data)
        {
            int result = 0;
            try
            {
                result = await SignalsDataAccessLayer.WriteToContactUs(SqlConnection, data);
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "ContactUs", ex);
            }
            return Ok(result);
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

        //private IEnumerable<string> GetAgencies(ExcelWorksheet sheet)
        //{
        //    return GetSingleColumnFromSpreadsheet(sheet, 6).Distinct().OrderBy(x => x);
        //}

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
                MetricsDataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    $"signals/getsinglecolumnfromspreadsheet/{col}", ex).GetAwaiter();
            }

            return new List<string>();
        }

        

        #endregion
        
    }
}
