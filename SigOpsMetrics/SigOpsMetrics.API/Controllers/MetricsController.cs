using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.DTOs;

namespace SigOpsMetrics.API.Controllers
{
    /// <summary>
    /// Controller for SigOps Metrics ATSPM data
    /// </summary>
    [ApiController]
    [Route("metrics")]
    public class MetricsController : _BaseController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="connection"></param>
        /// <param name="cache"></param>
        public MetricsController(IOptions<AppConfig> settings, MySqlConnection connection, IMemoryCache cache) : base(settings, connection, cache)
        {

        }

        #region Endpoints

        /// <summary>
        /// API method for returning high-level metric data from SigOpsMetrics.com
        /// </summary>
        /// <param name="source">One of {main, staging, beta}. main is the production data. staging is an advance preview of production from the 5th to the 15th of each month. beta is updated nightly, but isn't guaranteed to be available and may have errors.</param>
        /// <param name="level">One of {cor, sub, sig} for Corridor, Subcorridor or Signal-level data</param>
        /// <param name="interval">One of {qu, mo, wk, dy} for Quarterly, Monthly, Weekly or Daily data. Note that not all measures are aggregated at all levels.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="start">Start date for data pull</param>
        /// <param name="end">End date for data pull</param>
        /// <returns></returns>
        [HttpGet("")]
        //[ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<DataTable> Get(string source, string level, string interval, string measure, DateTime start,
            DateTime end)
        {
            var cacheName = $"Metrics/{source}/{level}/{interval}/{measure}/{start}/{end}";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = OneHourCache;

                    var dt = await DataAccessLayer.GetMetric(SqlConnection, source, level, interval, measure, start,
                        end);
                    return dt;
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
        /// API method for returning high-level metric data from SigOpsMetrics.com
        /// </summary>
        /// <param name="source">One of {main, staging, beta}. main is the production data. staging is an advance preview of production from the 5th to the 15th of each month. beta is updated nightly, but isn't guaranteed to be available and may have errors.</param>
        /// <param name="level">One of {cor, sub, sig} for Corridor, Subcorridor or Signal-level data</param>
        /// <param name="interval">One of {qu, mo, wk, dy} for Quarterly, Monthly, Weekly or Daily data. Note that not all measures are aggregated at all levels.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="start">Start date for data pull</param>
        /// <param name="end">End date for data pull</param>
        /// <param name="metric">The data metric field</param>
        /// <returns></returns>
        [HttpGet("signals")]
        //[ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<System.Collections.Generic.IEnumerable<SignalDTO>> GetSignalData(string source, string level, string interval, string measure, DateTime start, DateTime end, string metric)
        {
            var cacheName = $"Metrics/Signals/{source}/{level}/{interval}/{measure}/{start}/{end}";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = OneHourCache;

                    var dt = await DataAccessLayer.GetSignalMetricDataSQL(SqlConnection, source, level, interval, measure, start, end, metric);

                    return dt;
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
        /// API method for returning metric data by zone group from SigOpsMetrics.com
        /// </summary>
        /// <param name="source">One of {main, staging, beta}. main is the production data. staging is an advance preview of production from the 5th to the 15th of each month. beta is updated nightly, but isn't guaranteed to be available and may have errors.</param>
        /// <param name="level">One of {cor, sub, sig} for Corridor, Subcorridor or Signal-level data</param>
        /// <param name="interval">One of {qu, mo, wk, dy} for Quarterly, Monthly, Weekly or Daily data. Note that not all measures are aggregated at all levels.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="start">Start date for data pull</param>
        /// <param name="end">End date for data pull</param>
        /// <param name="zoneGroup">Zone Group (aka Signal Group) to pull data for</param>
        /// <returns></returns>
        [HttpGet(("zonegroup"))]
        //[ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<DataTable> GetByZoneGroup(string source, string level, string interval, string measure,
            DateTime start, DateTime end, string zoneGroup)
        {
            var cacheName = $"Metrics/ZoneGroup/{source}/{level}/{interval}/{measure}/{start}/{end}/{zoneGroup}";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = OneHourCache;

                    var dt = await DataAccessLayer.GetMetricByZoneGroup(SqlConnection, source, level, interval, measure,
                        start, end, zoneGroup);
                    return dt;
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
        /// API method for returning metric data by corridor from SigOpsMetrics.com
        /// </summary>
        /// <param name="source">One of {main, staging, beta}. main is the production data. staging is an advance preview of production from the 5th to the 15th of each month. beta is updated nightly, but isn't guaranteed to be available and may have errors.</param>
        /// <param name="level">One of {cor, sub, sig} for Corridor, Subcorridor or Signal-level data</param>
        /// <param name="interval">One of {qu, mo, wk, dy} for Quarterly, Monthly, Weekly or Daily data. Note that not all measures are aggregated at all levels.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="start">Start date for data pull</param>
        /// <param name="end">End date for data pull</param>
        /// <param name="corridor">Corridor to pull data for</param>
        /// <returns></returns>
        [HttpGet(("corridor"))]
        //[ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<DataTable> GetByCorridor(string source, string level, string interval, string measure,
            DateTime start, DateTime end, string corridor)
        {
            var cacheName = $"Metrics/Corridor/{source}/{level}/{interval}/{measure}/{start}/{end}/{corridor}";
            try
            {
                var cacheEntry = Cache.GetOrCreate(cacheName, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = OneHourCache;

                    var dt = await DataAccessLayer.GetMetricByCorridor(SqlConnection, source, level, interval, measure,
                        start, end, corridor);
                    return dt;
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

    }
}
