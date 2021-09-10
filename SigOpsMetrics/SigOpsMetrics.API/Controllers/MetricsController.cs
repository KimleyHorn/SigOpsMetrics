using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.DTOs;
using SigOpsMetrics.API.Classes.Extensions;
using SigOpsMetrics.API.Classes.Internal;
using SigOpsMetrics.API.DataAccess;

namespace SigOpsMetrics.API.Controllers
{
    /// <summary>
    /// Controller for SigOps Metrics ATSPM data
    /// </summary>
    [EnableCors("_myAllowSpecificOrigins")]
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

                    var dt = await MetricsDataAccessLayer.GetMetric(SqlConnection, source, level, interval, measure, start,
                        end);
                    return dt;
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

                    var dt = await MetricsDataAccessLayer.GetMetricByZoneGroup(SqlConnection, source, level, interval, measure,
                        start, end, zoneGroup);
                    return dt;
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

                    var dt = await MetricsDataAccessLayer.GetMetricByCorridor(SqlConnection, source, level, interval, measure,
                        start, end, corridor);
                    return dt;
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
        /// 
        /// </summary>
        /// <param name="source">>One of {main, staging, beta}. main is the production data. staging is an advance preview of production from the 5th to the 15th of each month. beta is updated nightly, but isn't guaranteed to be available and may have errors.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="filter">Filter object from the SPA</param>
        /// <returns></returns>
        [HttpPost("filter")]
        public async Task<DataTable> GetWithFilter(string source, string measure, [FromBody] FilterDTO filter)
        {
            return await GetFilteredDataTable(source, measure, filter);
        }

        [HttpPost("signals/filter")]
        public async Task<DataTable> GetSignalsByFilter(string source, string measure, [FromBody]
            FilterDTO filter)
        {
            var retVal = await GetFilteredDataTable(source, measure, filter, true);
            return retVal;
        }

        [HttpPost("signals/filter/average")]
        public async Task<List<AverageDTO>> GetSignalsAverageByFilter(string source, string measure, [FromBody]
            FilterDTO filter)
        {
            var retVal = await GetFilteredDataTable(source, measure, filter, true);
            List<AverageDTO> groupedData = new List<AverageDTO>();

            var avgColIndex = 2;
            var deltaColIndex = 3;
            var idCol = 0;

            //todo:some signals have different column orders than corridors - add here as we find them
            switch (measure)
            {
                case "vphpa":
                case "vphpp":
                    avgColIndex = 3;
                    deltaColIndex = 4;
                    break;
                case "pau":
                case "cu":
                    idCol = 1;
                    avgColIndex = 3;
                    deltaColIndex = 4;
                    break;
                case "du":
                    idCol = 1;
                    avgColIndex = 3;
                    deltaColIndex = 6;
                    break;
                case "cctv":
                    avgColIndex = 4;
                    deltaColIndex = 5;
                    break;
            }

            groupedData = (from row in retVal.AsEnumerable()
                           group row by new { label = row[idCol].ToString() } into g
                           select new AverageDTO
                           {
                               label = g.Key.label,
                               avg = g.Average(x => x[avgColIndex].ToDouble()),
                               delta = g.Average(x => x[deltaColIndex].ToDouble())
                           }).ToList();

            return groupedData;
        }

        [HttpPost("average")]
        public async Task<List<AverageDTO>> GetAverage(string source, string measure, bool dashboard, [FromBody]FilterDTO filter)
        {
            var retVal = await GetFilteredDataTable(source, measure, filter);
            List<AverageDTO> groupedData = new List<AverageDTO>();

            var avgColIndex = 3;
            var deltaColIndex = 5;

            if (measure == "vphpa" || measure == "vphpp")
            {
                avgColIndex = 3;
                deltaColIndex = 4;
            }

            if (dashboard)
            {
                var avg = (from row in retVal.AsEnumerable()
                           select row[avgColIndex].ToDouble()).Average();
                var delta = (from row in retVal.AsEnumerable()
                             select row[deltaColIndex].ToDouble()).Average();

                var data = new AverageDTO
                {
                    label = "Dashboard",
                    avg = avg,
                    delta = delta
                };
                groupedData.Add(data);
            }
            else
            {
                groupedData = (from row in retVal.AsEnumerable()
                                  group row by new { label = row[0].ToString() } into g
                                  select new AverageDTO
                                  {
                                      label = g.Key.label,
                                      avg = g.Average(x => x[avgColIndex].ToDouble()),
                                      delta = g.Average(x => x[deltaColIndex].ToDouble())
                                  }).ToList();
            }

            return groupedData.ToList();
        }

        private async Task<DataTable> GetFilteredDataTable(string source, string measure, [FromBody] FilterDTO filter, bool signalOnly = false)
        {
            // Check for an invalid filter
            //todo more checks as we start using the filter
            if (filter.timePeriod < 0) return null;

            //Quarterly data is formatted differently
            var interval = GetIntervalFromFilter(filter);
            var dates = GenerateDateFilter(filter);
            string startQuarter = null, endQuarter = null;
            if (interval == "qu")
            {
                startQuarter = dates.Item1.NearestQuarterEnd();
                endQuarter = dates.Item2.NearestQuarterEnd();
            }

            var filteredItems = new FilteredItems();
            if (signalOnly)
            {
                filteredItems = await SignalsDataAccessLayer.GetSignalsByFilter(SqlConnection, filter);
            }
            else
            {
                filteredItems = await SignalsDataAccessLayer.GetCorridorsOrSignalsByFilter(SqlConnection, filter);
            }

            //If we got no corridors/signals, bail
            if (filteredItems.Items.Any())
            {
                if (interval == "qu" && startQuarter != null && endQuarter != null)
                {
                    var retVal = await MetricsDataAccessLayer.GetMetricByFilter(SqlConnection, source, measure, interval,
                        startQuarter, endQuarter, filteredItems);

                    //var quarterCol = retVal.Columns["quarter"];
                    retVal.Columns["quarter"].MaxLength = 30;

                    foreach (DataRow row in retVal.Rows)
                    {
                        string cellData = row["quarter"].ToString();
                        var year = cellData.Substring(0, 4);
                        var quarter = cellData.Substring(5, 1);
                        var newDate = new DateTime(year.ToInt(), quarter.ToInt() * 3, 30);
                        row["quarter"] = newDate;
                    }

                    return retVal;
                }
                else
                {
                    var retVal =
                        MetricsDataAccessLayer.GetMetricByFilter(SqlConnection, source, measure, interval, dates.Item1.ToString(),
                            dates.Item2.ToString(), filteredItems);
                    return await retVal;
                }

            }

            return null;
        }

        private (DateTime, DateTime) GenerateDateFilter(FilterDTO filter)
        {
            var dt = DateTime.Today;

            var fullStart = new DateTime(dt.Year, dt.Month - 1, 1);
            var fullEnd = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));

            if (filter.dateRange != null)
            {
                switch ((GenericEnums.DateRangeType)filter.dateRange)
                {
                    case GenericEnums.DateRangeType.PriorDay:
                        fullStart = dt.AddDays(-1);
                        fullEnd = dt;
                        break;
                    case GenericEnums.DateRangeType.PriorWeek:
                        while (dt.DayOfWeek != DayOfWeek.Sunday)
                            dt = dt.AddDays(-1);

                        fullStart = dt.AddDays(-7);
                        fullEnd = dt.AddDays(-1);
                        break;
                    case GenericEnums.DateRangeType.PriorQuarter:
                        var month = (int)Math.Ceiling((double)dt.AddMonths(-3).Month / 3);
                        fullStart = new DateTime(dt.Year, month, 1);
                        fullEnd = new DateTime(dt.Year, month + 2, DateTime.DaysInMonth(dt.Year, month + 2));
                        break;
                    case GenericEnums.DateRangeType.PriorYear:
                        var priorYear = dt.Year - 1;
                        fullStart = new DateTime(priorYear, dt.Month, dt.Day);
                        fullEnd = dt;
                        break;
                    case GenericEnums.DateRangeType.Custom:
                        //TODO: add time range filter
                        fullStart = Convert.ToDateTime(filter.customStart);
                        fullEnd = Convert.ToDateTime(filter.customEnd);
                        break;
                    case GenericEnums.DateRangeType.PriorMonth:
                    default:
                        var priorMonth = dt.Month - 1;
                        fullStart = new DateTime(dt.Year, priorMonth, 1);
                        fullEnd = new DateTime(dt.Year, priorMonth, DateTime.DaysInMonth(dt.Year, priorMonth));
                        break;
                }
            }

            return (fullStart, fullEnd);
        }

        private string GetIntervalFromFilter(FilterDTO filter)
        {
            var aggregationType = (GenericEnums.DataAggregationType) filter.timePeriod;
            return EnumDescriptions.GetDescriptionFromEnumValue(aggregationType);
        }

        #endregion
        
    }
}
