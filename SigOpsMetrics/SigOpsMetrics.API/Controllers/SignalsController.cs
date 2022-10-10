﻿using Microsoft.AspNetCore.Mvc;
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
using Microsoft.Extensions.Configuration;
using SigOpsMetrics.API.DataAccess;
using SigOpsMetrics.API.Classes.Internal;

namespace SigOpsMetrics.API.Controllers
{
    /// <summary>
    /// Controller for signals, corridors, and groups
    /// </summary>
    [ApiController]
    [Route("signals")]
    public class SignalsController : _BaseController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="connection"></param>
        /// <param name="cache"></param>
        public SignalsController(IOptions<AppConfig> settings, IConfiguration configuration) : base(settings, configuration)
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
        public async Task<IEnumerable<SignalDTO>> Get()
        {
            try
            {
                return await SignalsDataAccessLayer.GetAllSignalDataSQL(SqlConnectionReader);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/all", ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of signal names in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("names")]
        public async Task<IEnumerable<string>> GetNames()
        {
            try
            {
                return await SignalsDataAccessLayer.GetSignalNamesSQL(SqlConnectionReader);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/names", ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of zone groups in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("zonegroups")]
        public async Task<IEnumerable<string>> GetZoneGroups()
        {
            try
            {
                return await SignalsDataAccessLayer.GetZoneGroupsSQL(SqlConnectionReader);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/zonegroups", ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of zones in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("zones")]
        public async Task<IEnumerable<string>> GetZones()
        {
            try
            {
                return await SignalsDataAccessLayer.GetZonesSQL(SqlConnectionReader);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/zones", ex);
                return null;
            }
        }

        [HttpGet("zonesbyzonegroup/{zoneGroup}")]
        public async Task<IEnumerable<string>> GetZonesByZoneGroup(string zoneGroup)
        {
            try
            {
                return await SignalsDataAccessLayer.GetZonesByZoneGroupSQL(SqlConnectionReader, zoneGroup);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/zonesbyzonegroup/{zoneGroup}", ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of corridors in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("corridors")]
        public async Task<IEnumerable<string>> GetCorridors()
        {
            try
            {
                return await SignalsDataAccessLayer.GetCorridorsSQL(SqlConnectionReader);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/corridors", ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of corridors by zone in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("corridorsbyzone/{zone}")]
        public async Task<IEnumerable<string>> GetCorridorsByZone(string zone)
        {
            try
            {
                return await SignalsDataAccessLayer.GetCorridorsByZoneSQL(SqlConnectionReader, zone);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/corridorsbyzone/{zone}", ex);
                return null;
            }
        }

        [HttpGet("corridorsbyzonegroup/{zoneGroup}")]
        public async Task<IEnumerable<string>> GetCorridorsByZoneGroup(string zoneGroup)
        {
            try
            {
                return await SignalsDataAccessLayer.GetCorridorsByZoneGroupSQL(SqlConnectionReader, zoneGroup);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "corridorsbyzonegroup/{zoneGroup}", ex);
                return null;
            }
        }

        /// <summary>
        /// Returns a list of corridors filtered by parts of the signal
        /// </summary>
        /// <returns></returns>
        [HttpGet("corridorsbyfilter")]
        public async Task<IEnumerable<string>> GetCorridorsByFilter(string zoneGroup, string zone, string agency, string county, string city)
        {
            try
            {
                FilterDTO filter = new FilterDTO()
                {
                    zone_Group = zoneGroup,
                    zone = zone,
                    agency = agency,
                    county = county,
                    city = city
                };
                FilteredItems results = await SignalsDataAccessLayer.GetCorridorsByFilter(SqlConnectionReader, filter);
                return results.Items;
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "corridorsbyzonegroup/{zoneGroup}", ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of subcorridors in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("subcorridors")]
        public async Task<IEnumerable<string>> GetSubCorridors()
        {
            try
            {
                return await SignalsDataAccessLayer.GetSubCorridorsSQL(SqlConnectionReader);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/subcorridors", ex);
                return null;
            }
        }

        /// <summary>
        /// Return a list of subcorridors by corridor in the system
        /// </summary>
        /// <returns></returns>
        [HttpGet("subcorridorsbycorridor/{corridor}")]
        public async Task<IEnumerable<string>> GetSubCorridorsByCorridor(string corridor)
        {
            try
            {
                //Decoding to handle corridors with / in them
                corridor = System.Web.HttpUtility.UrlDecode(corridor);
                return await SignalsDataAccessLayer.GetSubCorridorsByCorridorSQL(SqlConnectionReader, corridor);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/subcorridorsbycorridor/{corridor}", ex);
                return null;
            }
        }

        /// <summary>
        /// Return list of all agencies in the system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("agencies")]
        public async Task<IEnumerable<string>> GetAgencies()
        {
            try
            {
                return await SignalsDataAccessLayer.GetAgenciesSQL(SqlConnectionReader);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/agencies", ex);
                return null;
            }
        }

        /// <summary>
        /// Return list of all coutnies in the system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("counties")]
        public async Task<IEnumerable<string>> GetCounties()
        {
            try
            {
                return await SignalsDataAccessLayer.GetCountiesSQL(SqlConnectionReader);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/counties", ex);
                return null;
            }
        }

        /// <summary>
        /// Return list of all cities in system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("cities")]
        public async Task<IEnumerable<string>> GetCities()
        {
            try
            {
                return await SignalsDataAccessLayer.GetCitiesSQL(SqlConnectionReader);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "signals/cities", ex);
                return null;
            }
        }

        /// <summary>
        /// Endpoint for performing daily data pull of corridors_latest.xls into sql table.
        /// </summary>
        /// <returns></returns>
        [HttpGet("datapull/{key}")]
        public async Task DataPull(string key)
        {
            try
            {
                if (key == AppConfig.DataPullKey)
                {
                    Task<ExcelWorksheet> worksheet = GetSpreadsheet();
                    var ws = await worksheet;
                    await SignalsDataAccessLayer.WriteToSignals(SqlConnectionWriter, ws);
                }
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "DataPull", ex);
            }
        }

        /// <summary>
        /// Endpoint for submitting contact requests.
        /// </summary>
        /// <returns></returns>
        [HttpPost("contact-us")]
        public async Task<IActionResult> ContactUs(ContactInfo data)
        {
            int result = 0;
            try
            {
                result = await BaseDataAccessLayer.WriteToContactUs(SqlConnectionWriter, data);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
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
                Key = AppConfig.CorridorsKey
            };

            var ms = new MemoryStream();

            using (var getObjectResponse = await client.GetObjectAsync(request))
            {
                await getObjectResponse.ResponseStream.CopyToAsync(ms);
            }

            var package = new ExcelPackage(ms);
            return package.Workbook.Worksheets[0];
        }
        #endregion

    }
}