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

                    return await GetAllSignalDataSQL();
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

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetSignalNames(await worksheet);
                    return await GetSignalNamesSQL();

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

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetZoneGroups(await worksheet);
                    return await GetZoneGroupsSQL();

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

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetZones(await worksheet);
                    return await GetZonesSQL();
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
                    return await GetZonesByZoneGroupSQL(zoneGroup);
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

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetCorridors(await worksheet);
                    return await GetCorridorsSQL();
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
                    //return await GetCorridorsByZone(zone);

                    return await GetCorridorsByZoneSQL(zone);
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
                    return await GetCorridorsByZoneGroupSQL(zoneGroup);
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

                    //var worksheet = GetSpreadsheet();

                    //var retVal = GetSubCorridors(await worksheet);
                    return await GetSubCorridorsSQL();
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
                    return await GetSubCorridorsByCorridorSQL(corridor);
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
                    return await GetAgenciesSQL();
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
                    await DataAccessLayer.WriteToSignals(SqlConnection, ws);
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "DataPull", ex);
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

        private async Task<IEnumerable<SignalDTO>> GetAllSignalDataSQL()
        {
            List<SignalDTO> signals = new List<SignalDTO>();
            try
            {
                await SqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {

                    cmd.Connection = SqlConnection;
                    cmd.CommandText = "SELECT * FROM mark1.signals WHERE SignalID <> -1";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        SignalDTO row = new SignalDTO
                        {
                            SignalID = reader.IsDBNull(0) ? "" : reader.GetString(0).Trim(),
                            ZoneGroup = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim(),
                            Zone = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim(),
                            Corridor = reader.IsDBNull(3) ? "" : reader.GetString(3).Trim(),
                            Subcorridor = reader.IsDBNull(4) ? "" : reader.GetString(4).Trim(),
                            Agency = reader.IsDBNull(5) ? "" : reader.GetString(5).Trim(),
                            MainStreetName = reader.IsDBNull(6) ? "" : reader.GetString(6).Trim(),
                            SideStreetName = reader.IsDBNull(7) ? "" : reader.GetString(7).Trim(),
                            Milepost = reader.IsDBNull(8) ? "" : reader.GetString(8).Trim(),
                            AsOf = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                            Duplicate = reader.IsDBNull(10) ? "" : reader.GetString(10).Trim(),
                            Include = reader.IsDBNull(11) ? "" : reader.GetString(11).Trim(),
                            Modified = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12),
                            Note = reader.IsDBNull(13) ? "" : reader.GetString(13).Trim(),
                            Latitude = reader.IsDBNull(14) ? 0 : reader.GetDouble(14),
                            Longitude = reader.IsDBNull(15) ? 0 : reader.GetDouble(15),
                            County = reader.IsDBNull(16) ? "" : reader.GetString(16).Trim(),
                            City = reader.IsDBNull(17) ? "" : reader.GetString(17).Trim()
                        };
                        if (row.AsOf == DateTime.Parse("1899-12-31T00:00:00"))
                            row.AsOf = null;
                        if (row.Modified == DateTime.Parse("1899-12-31T00:00:00"))
                            row.Modified = null;
                        signals.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetAllSignalDataSQL", ex);
            }
            finally
            {
                SqlConnection.Close();
            }
            return signals;
        }

        //private IEnumerable<SignalDTO> GetAllSignalData(ExcelWorksheet sheet)
        //{
        //    try
        //    {
        //        var start = sheet.Dimension.Start;
        //        var end = sheet.Dimension.End;

        //        var retVal = new List<SignalDTO>();

        //        //todo error handling
        //        //Headers in row 1, data starts in row 2
        //        for (var row = start.Row + 1; row <= end.Row; row++)
        //        {
        //            //Excel is 1-based
        //            var col = 1;
        //            var newSignal = new SignalDTO
        //            {
        //                SignalID = sheet.Cells[row, col].Text,
        //                ZoneGroup = sheet.Cells[row, ++col].Text,
        //                Zone = sheet.Cells[row, ++col].Text,
        //                Corridor = sheet.Cells[row, ++col].Text,
        //                Subcorridor = sheet.Cells[row, ++col].Text,
        //                Agency = sheet.Cells[row, ++col].Text,
        //                MainStreetName = sheet.Cells[row, ++col].Text,
        //                SideStreetName = sheet.Cells[row, ++col].Text,
        //                Milepost = sheet.Cells[row, ++col].Text,
        //                AsOf = sheet.Cells[row, ++col].Text.ToNullableDateTime(),
        //                Duplicate = sheet.Cells[row, ++col].Text,
        //                Include = sheet.Cells[row, ++col].Text,
        //                Modified = sheet.Cells[row, ++col].Text.ToNullableDateTime(),
        //                Note = sheet.Cells[row, ++col].Text,
        //                Latitude = sheet.Cells[row, ++col].Text.ToDouble(),
        //                Longitude = sheet.Cells[row, ++col].Text.ToDouble()
        //            };
        //            retVal.Add(newSignal);
        //        }

        //        return retVal.Where(x => x.SignalID != "-1");
        //    }
        //    catch (Exception ex)
        //    {
        //        DataAccessLayer.WriteToErrorLog(SqlConnection,
        //            System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
        //            "signals/getallsignaldata", ex).GetAwaiter();
        //    }

        //    return new List<SignalDTO>();
        //}

        private async Task<IEnumerable<string>> GetSignalNamesSQL()
        {
            List<string> signals = new List<string>();
            try
            {
                await SqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {

                    cmd.Connection = SqlConnection;
                    cmd.CommandText = "SELECT CONCAT(TRIM(Main_Street_Name),' @ ', TRIM(Side_Street_Name)) FROM signals WHERE Main_Street_Name IS NOT NULL AND Side_Street_Name IS NOT NULL";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        signals.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetSignalNamesSQL", ex);
            }
            finally
            {
                SqlConnection.Close();
            }
            return signals;
        }

        //private IEnumerable<string> GetSignalNames(ExcelWorksheet sheet)
        //{
        //    try
        //    {
        //        var start = sheet.Dimension.Start;
        //        var end = sheet.Dimension.End;

        //        var retVal = new List<string>();

        //        for (var row = start.Row + 1; row <= end.Row; row++)
        //        {
        //            var priCol = 7;
        //            var secCol = 8;

        //            retVal.Add(sheet.Cells[row, priCol].Text.Trim() + " @ " + sheet.Cells[row, secCol].Text.Trim());
        //        }

        //        return retVal;
        //    }
        //    catch (Exception ex)
        //    {
        //        DataAccessLayer.WriteToErrorLog(SqlConnection,
        //            System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
        //            "signals/getsignalnames", ex).GetAwaiter();
        //    }

        //    return new List<string>();
        //}

        private async Task<IEnumerable<string>> GetZoneGroupsSQL()
        {
            List<string> zoneGroups = new List<string>();
            try
            {
                await SqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {

                    cmd.Connection = SqlConnection;
                    cmd.CommandText = "SELECT DISTINCT(Zone_Group) FROM signals WHERE Zone_Group IS NOT NULL ORDER BY Zone_Group ASC";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        zoneGroups.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetZoneGroupsSQL", ex);
            }
            finally
            {
                SqlConnection.Close();
            }
            return zoneGroups;
        }

        //private IEnumerable<string> GetZoneGroups(ExcelWorksheet sheet)
        //{
        //    var retVal = GetSingleColumnFromSpreadsheet(sheet, 2).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()
        //        .OrderBy(x => x).ToList();
        //    //retVal.Insert(0, "All RTOP"); //Hardcode in an 'all' option - will have to check for special case in Metrics controller
        //    return retVal;
        //}

        private async Task<IEnumerable<string>> GetZonesSQL()
        {
            List<string> zones = new List<string>();
            try
            {
                await SqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {

                    cmd.Connection = SqlConnection;
                    cmd.CommandText = "SELECT DISTINCT(Zone) FROM signals WHERE Zone IS NOT NULL ORDER BY Zone ASC";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        zones.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetZonesSQL", ex);
            }
            finally
            {
                SqlConnection.Close();
            }
            return zones;
        }


        private async Task<IEnumerable<string>> GetZonesByZoneGroupSQL(string zoneGroupName)
        {
            List<string> zones = new List<string>();
            try
            {
                await SqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = SqlConnection;
                    cmd.CommandText = "SELECT DISTINCT(Zone) FROM signals WHERE Zone IS NOT NULL AND TRIM(UPPER(Zone_Group))";
                    string where = "";
                    switch (zoneGroupName.Trim().ToUpper())
                    {
                        case "ALL RTOP":
                            where += " IN ('RTOP1','RTOP2')";
                            break;
                        case "Zone 7":
                            where += " IN ('ZONE 7M', 'ZONE 7D')";
                            break;
                        default:
                            where += " = @zoneGroupName";
                            cmd.Parameters.AddWithValue("zoneGroupName", zoneGroupName.Trim().ToUpper());
                            break;
                    }
                    cmd.CommandText += where;

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        zones.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetZonesByZoneGroupSQL", ex);
            }
            finally
            {
                SqlConnection.Close();
            }
            return zones;
        }

        //private IEnumerable<string> GetZonesByZoneGroup(ExcelWorksheet sheet, string zoneGroupName)
        //{
        //    try
        //    {
        //        var start = sheet.Dimension.Start;
        //        var end = sheet.Dimension.End;

        //        var retVal = new List<string>();

        //        for (var row = start.Row + 1; row <= end.Row; row++)
        //        {
        //            if(zoneGroupName == "All RTOP")
        //            {
        //                if (string.Equals(sheet.Cells[row, 2].Text.Trim(), "RTOP1", StringComparison.CurrentCultureIgnoreCase) 
        //                    || string.Equals(sheet.Cells[row, 2].Text.Trim(), "RTOP2", StringComparison.CurrentCultureIgnoreCase))
        //                    retVal.Add(sheet.Cells[row, 3].Text.Trim());
        //            }
        //            else if (zoneGroupName == "Zone 7")
        //            {
        //                if (string.Equals(sheet.Cells[row, 2].Text.Trim(), "Zone 7m", StringComparison.CurrentCultureIgnoreCase) 
        //                    || string.Equals(sheet.Cells[row, 2].Text.Trim(), "Zone 7d", StringComparison.CurrentCultureIgnoreCase))
        //                    retVal.Add(sheet.Cells[row, 3].Text.Trim());
        //            }
        //            else
        //            {
        //                if (string.Equals(sheet.Cells[row, 2].Text.Trim(), zoneGroupName,
        //                StringComparison.CurrentCultureIgnoreCase))
        //                    retVal.Add(sheet.Cells[row, 3].Text.Trim());
        //            }
        //        }

        //        return retVal.Distinct();
        //    }
        //    catch (Exception ex)
        //    {
        //        DataAccessLayer.WriteToErrorLog(SqlConnection,
        //            System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
        //            "signals/getzonesbyzonegroup", ex).GetAwaiter();
        //    }

        //    return new List<string>();
        //}

        private async Task<IEnumerable<string>> GetCorridorsByZoneGroupSQL(string zoneGroupName)
        {
            List<string> zones = new List<string>();
            try
            {
                await SqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = SqlConnection;
                    cmd.CommandText = "SELECT DISTINCT(Corridor) FROM signals WHERE Corridor IS NOT NULL AND TRIM(UPPER(Zone_Group))";
                    string where = "";
                    switch (zoneGroupName.Trim().ToUpper())
                    {
                        case "ALL RTOP":
                            where += " IN ('RTOP1','RTOP2')";
                            break;
                        case "Zone 7":
                            where += " IN ('ZONE 7M', 'ZONE 7D')";
                            break;
                        default:
                            where += " = @zoneGroupName";
                            cmd.Parameters.AddWithValue("zoneGroupName", zoneGroupName.Trim().ToUpper());
                            break;
                    }
                    cmd.CommandText += where;

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        zones.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetCorridorsByZoneGroupSQL", ex);
            }
            finally
            {
                SqlConnection.Close();
            }
            return zones;
        }

        //private IEnumerable<string> GetCorridorsByZoneGroup(ExcelWorksheet sheet, string zoneGroupName)
        //{
        //    try
        //    {
        //        var start = sheet.Dimension.Start;
        //        var end = sheet.Dimension.End;

        //        var retVal = new List<string>();

        //        for (var row = start.Row + 1; row <= end.Row; row++)
        //        {
        //            if (zoneGroupName == "All RTOP")
        //            {
        //                if (string.Equals(sheet.Cells[row, 2].Text.Trim(), "RTOP1", StringComparison.CurrentCultureIgnoreCase)
        //                    || string.Equals(sheet.Cells[row, 2].Text.Trim(), "RTOP2", StringComparison.CurrentCultureIgnoreCase))
        //                    retVal.Add(sheet.Cells[row, 4].Text.Trim());
        //            }
        //            else if (zoneGroupName == "Zone 7")
        //            {
        //                if (string.Equals(sheet.Cells[row, 2].Text.Trim(), "Zone 7m", StringComparison.CurrentCultureIgnoreCase)
        //                    || string.Equals(sheet.Cells[row, 2].Text.Trim(), "Zone 7d", StringComparison.CurrentCultureIgnoreCase))
        //                    retVal.Add(sheet.Cells[row, 4].Text.Trim());
        //            }
        //            else
        //            {
        //                if (string.Equals(sheet.Cells[row, 2].Text.Trim(), zoneGroupName,
        //                StringComparison.CurrentCultureIgnoreCase))
        //                    retVal.Add(sheet.Cells[row, 4].Text.Trim());
        //            }
        //        }

        //        return retVal.Distinct();
        //    }
        //    catch (Exception ex)
        //    {
        //        DataAccessLayer.WriteToErrorLog(SqlConnection,
        //            System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
        //            "signals/getcorridorsbyzonegroup", ex).GetAwaiter();
        //    }

        //    return new List<string>();
        //}

        private async Task<IEnumerable<string>> GetCorridorsSQL()
        {
            List<string> corridors = new List<string>();
            try
            {
                await SqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = SqlConnection;
                    cmd.CommandText = "SELECT DISTINCT(Corridor) FROM signals WHERE Corridor IS NOT NULL";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        corridors.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetCorridorsSQL", ex);
            }
            finally
            {
                SqlConnection.Close();
            }
            return corridors;
        }

        //private IEnumerable<string> GetCorridors(ExcelWorksheet sheet)
        //{
        //    return GetSingleColumnFromSpreadsheet(sheet, 4).Distinct().OrderBy(x => x);
        //}

        private async Task<IEnumerable<string>> GetCorridorsByZoneSQL(string zoneName)
        {
            List<string> corridors = new List<string>();
            try
            {
                await SqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = SqlConnection;
                    cmd.CommandText = "SELECT DISTINCT(Corridor) FROM signals WHERE TRIM(Zone) = @zoneName";
                    cmd.Parameters.AddWithValue("zoneName", zoneName.Trim());

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        corridors.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetCorridorsByZoneSQL", ex);
            }
            finally
            {
                SqlConnection.Close();
            }
            return corridors;
        }

        //private IEnumerable<string> GetCorridorsByZone(ExcelWorksheet sheet, string zoneName)
        //{
        //    try
        //    {
        //        var start = sheet.Dimension.Start;
        //        var end = sheet.Dimension.End;

        //        var retVal = new List<string>();

        //        for (var row = start.Row + 1; row <= end.Row; row++)
        //        {
        //            if (string.Equals(sheet.Cells[row, 3].Text.Trim(), zoneName,
        //                StringComparison.CurrentCultureIgnoreCase))
        //                retVal.Add(sheet.Cells[row, 4].Text.Trim());
        //        }

        //        return retVal.Distinct();
        //    }
        //    catch (Exception ex)
        //    {
        //        DataAccessLayer.WriteToErrorLog(SqlConnection,
        //            System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
        //            "signals/getcorridorsbyzone", ex).GetAwaiter();
        //    }

        //    return new List<string>();
        //}

        private async Task<IEnumerable<string>> GetSubCorridorsSQL()
        {
            List<string> subcorridors = new List<string>();
            try
            {
                await SqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = SqlConnection;
                    cmd.CommandText = "SELECT DISTINCT(Subcorridor) FROM signals WHERE Subcorridor IS NOT NULL";
                    
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        subcorridors.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetSubCorridorsSQL", ex);
            }
            finally
            {
                SqlConnection.Close();
            }
            return subcorridors;
        }

        //private IEnumerable<string> GetSubCorridors(ExcelWorksheet sheet)
        //{
        //    return GetSingleColumnFromSpreadsheet(sheet, 5).Distinct().OrderBy(x => x);
        //}

        private async Task<IEnumerable<string>> GetSubCorridorsByCorridorSQL(string corridor)
        {
            List<string> subCorridors = new List<string>();
            try
            {
                await SqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = SqlConnection;
                    cmd.CommandText = "SELECT DISTINCT(SubCorridor) FROM signals WHERE TRIM(Corridor) = @corridor AND Subcorridor IS NOT NULL";
                    cmd.Parameters.AddWithValue("corridor", corridor.Trim());

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        subCorridors.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetSubCorridorsByCorridorSQL", ex);
            }
            finally
            {
                SqlConnection.Close();
            }
            return subCorridors;
        }

        //private IEnumerable<string> GetSubCorridorsByCorridor(ExcelWorksheet sheet, string corridor)
        //{
        //    try
        //    {
        //        var start = sheet.Dimension.Start;
        //        var end = sheet.Dimension.End;

        //        var retVal = new List<string>();

        //        for (var row = start.Row + 1; row <= end.Row; row++)
        //        {
        //            if (string.Equals(sheet.Cells[row, 4].Text.Trim(), corridor,
        //                StringComparison.CurrentCultureIgnoreCase))
        //                retVal.Add(sheet.Cells[row, 5].Text.Trim());
        //        }

        //        return retVal.Distinct();
        //    }
        //    catch (Exception ex)
        //    {
        //        DataAccessLayer.WriteToErrorLog(SqlConnection,
        //            System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
        //            "signals/getsubcorridorsbycorridor", ex).GetAwaiter();
        //    }

        //    return new List<string>();
        //}

        private async Task<IEnumerable<string>> GetAgenciesSQL()
        {
            List<string> agencies = new List<string>();
            try
            {
                await SqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = SqlConnection;
                    cmd.CommandText = "SELECT DISTINCT(Agency) FROM signals WHERE Agency IS NOT NULL";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        agencies.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await DataAccessLayer.WriteToErrorLog(SqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetAgenciesSQL", ex);
            }
            finally
            {
                SqlConnection.Close();
            }
            return agencies;
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
                DataAccessLayer.WriteToErrorLog(SqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    $"signals/getsinglecolumnfromspreadsheet/{col}", ex).GetAwaiter();
            }

            return new List<string>();
        }

        

        #endregion
        
    }
}
