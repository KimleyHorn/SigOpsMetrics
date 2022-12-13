using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MySqlConnector;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.DTOs;
using SigOpsMetrics.API.Classes.Extensions;
using SigOpsMetrics.API.DataAccess;

namespace SigOpsMetrics.API.Controllers
{
    [Route("metrics/csv")]
    [ApiController]
    public class ExportMetricsController : _BaseController
    {
        public ExportMetricsController(IOptions<AppConfig> settings, IConfiguration configuration) : base(settings, configuration)
        {

        }

        /// <summary>
        /// API method for returning high-level metric data from SigOpsMetrics.com in CSV format.
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
        public async Task<FileStreamResult> Get(string source, string level, string interval, string measure, DateTime start,
            DateTime end)
        {
            try
            {
                var dt = await MetricsDataAccessLayer.GetMetric(SqlConnectionReader, source, level, interval, measure, start, end);
                return File(StreamExtensions.ConvertToCSV(dt), "text/plain","data.csv");
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnectionWriter, System.Reflection.Assembly.GetEntryAssembly().GetName().Name, "metrics/csv/get", ex);
                return null;
            }
        }

        /// <summary>
        /// API method for returning metric data by zone group from SigOpsMetrics.com in CSV format.
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
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<FileStreamResult> GetByZoneGroup(string source, string level, string interval, string measure,
            DateTime start, DateTime end, string zoneGroup)
        {
            try
            {
                var dt = await MetricsDataAccessLayer.GetMetricByZoneGroup(SqlConnectionReader, source, level, interval, measure, start, end, zoneGroup);
                return File(StreamExtensions.ConvertToCSV(dt), "text/plain", "data.csv");
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnectionWriter, System.Reflection.Assembly.GetEntryAssembly().GetName().Name, "metrics/csv/zonegroup", ex);
                return null;
            }
        }

        /// <summary>
        /// API method for returning metric data by corridor from SigOpsMetrics.com in CSV format.
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
        [ResponseCache(CacheProfileName = CacheProfiles.Default)]
        public async Task<FileStreamResult> GetByCorridor(string source, string level, string interval, string measure,
            DateTime start, DateTime end, string corridor)
        {
            try
            {
                var dt = await MetricsDataAccessLayer.GetMetricByCorridor(SqlConnectionReader, source, level, interval, measure, start, end, corridor);
                return File(StreamExtensions.ConvertToCSV(dt), "text/plain", "data.csv");
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnectionWriter, System.Reflection.Assembly.GetEntryAssembly().GetName().Name,"metrics/csv/corridor", ex);
                return null;
            }
        }

        /// <summary>
        /// API method for returning metric data by corridor from SigOpsMetrics.com in CSV format.
        /// </summary>
        /// <param name="source">>One of {main, staging, beta}. main is the production data. staging is an advance preview of production from the 5th to the 15th of each month. beta is updated nightly, but isn't guaranteed to be available and may have errors.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="filter">Filter object from the SPA</param>
        /// <returns></returns>
        [HttpPost("filter")]
        public async Task<FileStreamResult> GetWithFilter(string source, string measure, [FromBody] FilterDTO filter)
        {
            try
            {
                MetricsDataAccessLayer metricsData = new MetricsDataAccessLayer();
                var retVal = await metricsData.GetFilteredDataTable(source, measure, filter, SqlConnectionReader, SqlConnectionWriter);
                return File(StreamExtensions.ConvertToCSV(retVal), "text/plain", "data.csv");

            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnectionWriter, System.Reflection.Assembly.GetEntryAssembly().GetName().Name, "metrics/csv/filter", ex);
                return null;
            }
        }

        /// <summary>
        /// API method for returning filtered signal data in CSV format.
        /// </summary>
        /// <param name="source">>One of {main, staging, beta}. main is the production data. staging is an advance preview of production from the 5th to the 15th of each month. beta is updated nightly, but isn't guaranteed to be available and may have errors.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="filter">Filter object from the SPA</param>
        /// <returns></returns>
        [HttpPost("signals/filter")]
        public async Task<FileStreamResult> GetSignalsByFilter(string source, string measure, [FromBody]
            FilterDTO filter)
        {
            try
            {
                MetricsDataAccessLayer metricsData = new MetricsDataAccessLayer();
                var retVal = await metricsData.GetFilteredDataTable(source, measure, filter, SqlConnectionReader, SqlConnectionWriter, true);
                return File(StreamExtensions.ConvertToCSV(retVal), "text/plain", "data.csv");
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnectionWriter, System.Reflection.Assembly.GetEntryAssembly().GetName().Name, "metrics/csv/signals/filter", ex);
                return null;
            }
        }

        /// <summary>
        /// API method for returning filtered average signal data in CSV format.
        /// </summary>
        /// <param name="source">>One of {main, staging, beta}. main is the production data. staging is an advance preview of production from the 5th to the 15th of each month. beta is updated nightly, but isn't guaranteed to be available and may have errors.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="filter">Filter object from the SPA</param>
        /// <returns></returns>
        [HttpPost("signals/filter/average")]
        public async Task<FileStreamResult> GetSignalsAverageByFilter(string source, string measure, [FromBody]
            FilterDTO filter)
        {
            try
            {
                MetricsDataAccessLayer metricsData = new MetricsDataAccessLayer();
                var retVal = await metricsData.GetFilteredDataTable(source, measure, filter, SqlConnectionReader, SqlConnectionWriter, true);
                List<AverageDTO> groupedData = new List<AverageDTO>();

                if (retVal == null || retVal.Rows.Count == 0)
                {
                    return null;
                }

                var indexes = metricsData.GetAvgDeltaIDColumnIndexes(filter, measure, false);

                var avgColIndex = indexes.avgColIndex;
                var deltaColIndex = indexes.deltaColIndex;
                var idColIndex = indexes.idColIndex;

                if (filter.zone_Group == "All")
                {
                    groupedData = (from row in retVal.AsEnumerable()
                                   group row by new { label = row[idColIndex].ToString(), zoneGroup = row["ActualZoneGroup"] } into g
                                   select new AverageDTO
                                   {
                                       label = g.Key.label,
                                       avg = g.Average(x => x[avgColIndex].ToDouble()),
                                       delta = g.Average(x => x[deltaColIndex].ToDouble()),
                                       zoneGroup = g.Key.zoneGroup.ToString()
                                   }).ToList();
                }
                else
                {
                    groupedData = (from row in retVal.AsEnumerable()
                                   group row by new { label = row[idColIndex].ToString() } into g
                                   select new AverageDTO
                                   {
                                       label = g.Key.label,
                                       avg = g.Average(x => x[avgColIndex].ToDouble()),
                                       delta = g.Average(x => x[deltaColIndex].ToDouble())
                                   }).ToList();
                }
                return File(StreamExtensions.ConvertToCSV(groupedData), "text/plain", "data.csv");

            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnectionWriter, System.Reflection.Assembly.GetEntryAssembly().GetName().Name, "metrics/csv/signals/filter/average", ex);
                return null;
            }
        }

        /// <summary>
        /// API method for returning average data is CSV format.
        /// </summary>
        /// <param name="source">>One of {main, staging, beta}. main is the production data. staging is an advance preview of production from the 5th to the 15th of each month. beta is updated nightly, but isn't guaranteed to be available and may have errors.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="dashboard">Format data for dashboard.</param>
        /// <param name="filter">Filter object from the SPA</param>
        /// <returns></returns>
        [HttpPost("average")]
        public async Task<FileStreamResult> GetAverage(string source, string measure, bool dashboard, [FromBody] FilterDTO filter)
        {
            try
            {
                MetricsDataAccessLayer metricsData = new MetricsDataAccessLayer();
                var isCorridor = true;
                var retVal = await metricsData.GetFilteredDataTable(source, measure, filter, SqlConnectionReader, SqlConnectionWriter);
                if (retVal.TableName.Contains("sig"))
                    isCorridor = false;

                var groupedData = new List<AverageDTO>();

                var indexes = metricsData.GetAvgDeltaIDColumnIndexes(filter, measure, isCorridor);

                var idColIndex = indexes.idColIndex;
                var avgColIndex = indexes.avgColIndex;
                var deltaColIndex = indexes.deltaColIndex;

                if (retVal == null || retVal.Rows.Count == 0)
                {
                    return null;
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
                else if (filter.zone_Group == "All")
                {
                    // group on zone_group instead of corridor
                    groupedData = (from row in retVal.AsEnumerable()
                                   group row by new { label = row[7].ToString() } into g
                                   select new AverageDTO
                                   {
                                       label = g.Key.label,
                                       avg = g.Average(x => x[avgColIndex].ToDouble()),
                                       delta = g.Average(x => x[deltaColIndex].ToDouble())
                                   }).ToList();
                }
                else
                {
                    groupedData = (from row in retVal.AsEnumerable()
                                   group row by new { label = row[idColIndex].ToString() } into g
                                   select new AverageDTO
                                   {
                                       label = g.Key.label,
                                       avg = g.Average(x => x[avgColIndex].ToDouble()),
                                       delta = g.Average(x => x[deltaColIndex].ToDouble())
                                   }).ToList();
                }

                return File(StreamExtensions.ConvertToCSV(groupedData), "text/plain", "data.csv");
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(SqlConnectionWriter, System.Reflection.Assembly.GetEntryAssembly().GetName().Name, "metrics/csv/average", ex);
                return null;
            }
        }
    }
}
