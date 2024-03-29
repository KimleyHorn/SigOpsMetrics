﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.DTOs;
using SigOpsMetrics.API.Classes.Extensions;
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
        public MetricsController(IOptions<AppConfig> settings, IConfiguration configuration) : base(settings, configuration)
        {

        }
        /// <summary>
        /// This API method has been deprecated. Use the GetWithFilter method instead.
        /// </summary>
        /// <param name="source">One of {main, staging, beta}. main is the production data. staging is an advance preview of production from the 5th to the 15th of each month. beta is updated nightly, but isn't guaranteed to be available and may have errors.</param>
        /// <param name="level">One of {cor, sub, sig} for Corridor, Subcorridor or Signal-level data</param>
        /// <param name="interval">One of {qu, mo, wk, dy} for Quarterly, Monthly, Weekly or Daily data. Note that not all measures are aggregated at all levels.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="start">Start date for data pull</param>
        /// <param name="end">End date for data pull</param>
        /// <returns></returns>
        [HttpGet("")]
        public async Task<DataTable> Get(string source, string level, string interval, string measure, DateTime start,
            DateTime end)
        {
            try
            {
                var dt = await MetricsDataAccessLayer.GetMetric(SqlConnectionReader, source, level, interval, measure, start,
                    end);
                return dt;
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "Metrics/Get", ex);
                return null;
            }
        }

        /// <summary>
        /// This API method has been deprecated. Use the GetWithFilter method instead.
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
        public async Task<DataTable> GetByZoneGroup(string source, string level, string interval, string measure,
            DateTime start, DateTime end, string zoneGroup)
        {
            try
            {
                var dt = await MetricsDataAccessLayer.GetMetricByZoneGroup(SqlConnectionReader, source, level, interval, measure,
                    start, end, zoneGroup);
                return dt;
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "metrics/zonegroups", ex);
                return null;
            }
        }

        /// <summary>
        /// This API method has been deprecated. Use the GetWithFilter method instead.
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
        public async Task<DataTable> GetByCorridor(string source, string level, string interval, string measure,
            DateTime start, DateTime end, string corridor)
        {
            try
            {
                var dt = await MetricsDataAccessLayer.GetMetricByCorridor(SqlConnectionReader, source, level, interval, measure,
                    start, end, corridor);
                return dt;
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "metrics/corridor", ex);
                return null;
            }
        }

        /// <summary>
        /// Primary API method for returning corridor/signal data from the SigOps Metrics database
        /// </summary>
        /// <remarks>
        /// This method will attempt to return data at a Corridor level, but will switch to Signal data if the filter is too restrictive. 
        /// Use the Signals\Filter method to explicitly ask for Signal level data
        /// </remarks>
        /// <param name="source">'main' is currently the only allowed source. The database is updated nightly.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="filter">Filter object</param>
        /// <returns></returns>
        [HttpPost("filter")]
        public async Task<DataTable> GetWithFilter(string source, string measure, [FromBody] FilterDTO filter)
        {
            // This is for bottom right graph
            try
            {
                // For health metrics, we want to show prior year, monthly
                if (measure.EndsWith("plot"))
                {
                    filter.timePeriod = 4;
                    filter.dateRange = 4;
                }
                MetricsDataAccessLayer metricsData = new MetricsDataAccessLayer();
                // Do this check here to prevent extra processing and database queries if the filter is not valid.
                string interval = metricsData.GetIntervalFromFilter(filter);
                if (!IsFilterValid(measure, interval))
                {
                    await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "metrics/filter", new Exception("Invalid filter."));
                }

                var retVal = await metricsData.GetFilteredDataTable(source, measure, filter, SqlConnectionReader, SqlConnectionWriter);

                return retVal;
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "metrics/filter", ex);
                return null;
            }
        }

        /// <summary>
        /// Get straight average and delta with no grouping
        /// </summary>
        /// <param name="source"></param>
        /// <param name="measure"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("straightaverage")]
        public async Task<AverageDTO> StraightAverage(string source, string measure, [FromBody] FilterDTO filter)
        {
            try
            {
                MetricsDataAccessLayer metricsData = new MetricsDataAccessLayer();
                var retVal = await metricsData.GetFilteredDataTable(source, measure, filter, SqlConnectionReader, SqlConnectionWriter, true);
                bool isCorridor = measure is "tti" or "pti";
                var indexes = metricsData.GetAvgDeltaIDColumnIndexes(filter, measure, isCorridor);

                var avgColIndex = indexes.avgColIndex;
                var deltaColIndex = indexes.deltaColIndex;

                var avg = (from row in retVal.AsEnumerable()
                    select row[avgColIndex].ToDouble()).Average();
                var delta = (from row in retVal.AsEnumerable()
                    select row[deltaColIndex].ToDouble()).Average();

                var dto = new AverageDTO
                {
                    avg = avg,
                    delta = Math.Round(delta, 3)
                };
                return dto;
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
    "metrics/straightaverage", ex);
                return null;
            }
        }

        [HttpPost("summarytrends")]
        public async Task<Dictionary<string, List<SummaryTrendDTO>>> GetSummaryTrendsWithFilter(string source, [FromBody] FilterDTO filter)
        {
            try
            {
                var metricsData = new MetricsDataAccessLayer();
                return await metricsData.GetSummaryTrend(source, filter, SqlConnectionReader, SqlConnectionWriter);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
    "metrics/summaryTrends", ex);
                return null;
            }
        }

        /// <summary>
        /// API method for returning Signal data from the SigOps Metrics database.
        /// </summary>
        /// <param name="source">'main' is currently the only allowed source. The database is updated nightly.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="filter">Filter object</param>
        /// <returns></returns>
        [HttpPost("signals/filter")]
        public async Task<DataTable> GetSignalsByFilter(string source, string measure, [FromBody]
            FilterDTO filter)
        {
            try
            {
                MetricsDataAccessLayer metricsData = new MetricsDataAccessLayer();
                var retVal = await metricsData.GetFilteredDataTable(source, measure, filter, SqlConnectionReader, SqlConnectionWriter, true);
                return retVal;
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "metrics/signals/filter", ex);
                return null;
            }
        }



        /// <summary>
        /// Returns a list of all flash events in the system
        /// </summary>
        /// <returns></returns>
        [HttpPost("flashevents")]
        public async Task<List<FlashEventDTO>> GetFlashEvents([FromBody] FilterDTO filter)
        {
            try
            {
                var flashData = new MetricsDataAccessLayer();
                return await flashData.GetFlashSignalsFromFilter(SqlConnectionReader, filter);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                                       System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                                                          "signals/flashevents", ex);
                return null;
            }
        }



        /// <summary>
        /// Returns a list of all preempt events in the system
        /// </summary>
        /// <returns></returns>
        [HttpPost("preemptevents")]
        public async Task<IEnumerable<PreemptEventDTO>> GetPreemptEvents([FromBody] FilterDTO filter)
        {
            try
            {
                var preData = new MetricsDataAccessLayer();
                return await preData.GetPreemptEventsFromFilter(SqlConnectionReader, filter);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// Returns the average value per signal based on the supplied filter.
        /// </summary>
        /// <param name="source">'main' is currently the only allowed source. The database is updated nightly.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="filter">Filter object</param>
        /// <returns></returns>
        [HttpPost("signals/filter/average")]
        public async Task<List<AverageDTO>> GetSignalsAverageByFilter(string source, string measure, [FromBody]
            FilterDTO filter)
        {
            // This is for the map signals
            try
            {
                MetricsDataAccessLayer metricsData = new MetricsDataAccessLayer();

                // Do this check here to prevent extra processing and database queries if the filter is not valid.
                string interval = metricsData.GetIntervalFromFilter(filter);
                if (!IsFilterValid(measure, interval))
                {
                    await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                        System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                        "metrics/signals/filter/average", new Exception("Invalid filter."));
                    return null;
                }

                var signals = await SignalsDataAccessLayer.GetSignalsByFilter(SqlConnectionReader, filter);

                var retVal = await metricsData.GetFilteredDataTable(source, measure, filter, SqlConnectionReader,
                    SqlConnectionWriter, true);
                List<AverageDTO> groupedData = new List<AverageDTO>();

                if (retVal == null || retVal.Rows.Count == 0)
                {
                    return groupedData.ToList();
                }

                var indexes = metricsData.GetAvgDeltaIDColumnIndexes(filter, measure, false);

                var avgColIndex = indexes.avgColIndex;
                var deltaColIndex = indexes.deltaColIndex;
                var idColIndex = indexes.idColIndex;

                if (filter.zone_Group == "All")
                {
                    groupedData = (from row in retVal.AsEnumerable()
                                   group row by new { label = row[idColIndex].ToString(), zoneGroup = row["Zone_Group"] } into g
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

                var includedIds = groupedData.Select(x => x.label);
                foreach (var unavailableSignal in signals.Items.Where(x => !includedIds.Contains(x)))
                {
                    groupedData.Add(new AverageDTO
                    {
                        avg = -1,
                        delta = -1,
                        label = unavailableSignal
                    });
                }
                return groupedData;
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "metrics/signals/filter/average", ex);
                return null;
            }
        }

        /// <summary>
        /// Primary API method for returning corridor/signal data from the SigOps Metrics database.
        /// </summary>
        /// <remarks>
        /// This method will attempt to return data at a Corridor level, but will switch to Signal data if the filter is too restrictive.
        /// Use the Signals\Filter method to explicitly ask for Signal level data.
        /// </remarks>
        /// <param name="source">'main' is currently the only allowed source. The database is updated nightly.</param>
        /// <param name="measure">See Measure Definitions above for possible values (e.g., vpd, aogd). Note that not all measures are calculated for all combinations of level and interval.</param>
        /// <param name="dashboard">Format data for dashboard.</param>
        /// <param name="filter">Filter object from the SPA</param>
        /// <returns></returns>
        [HttpPost("average")]
        public async Task<List<AverageDTO>> GetAverage(string source, string measure, bool dashboard, [FromBody] FilterDTO filter)
        {
            // This is for bottom left but also requires GetWithFilter
            try
            {
                // For health metrics, we want to show prior year, monthly
                if (measure.EndsWith("plot"))
                {
                    filter.timePeriod = 4;
                    filter.dateRange = 4;
                }
                var metricsData = new MetricsDataAccessLayer();

                // Do this check here to prevent extra processing and database queries if the filter is not valid.
                var interval = metricsData.GetIntervalFromFilter(filter);
                if (!IsFilterValid(measure, interval))
                {
                    await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "metrics/average", new Exception("Invalid filter."));
                    return null;
                }

                var isCorridor = true;
                var retVal = await metricsData.GetFilteredDataTable(source, measure, filter, SqlConnectionReader, SqlConnectionWriter);
                if (retVal != null && (!string.IsNullOrWhiteSpace(filter.corridor) || !string.IsNullOrWhiteSpace(filter.signalId)))
                    isCorridor = false;

                var groupedData = new List<AverageDTO>();

                var (idColIndex, avgColIndex, deltaColIndex) = metricsData.GetAvgDeltaIDColumnIndexes(filter, measure, isCorridor);

                if (retVal == null || retVal.Rows.Count == 0)
                {
                    return groupedData.ToList();
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
                    var total = (double)retVal.Rows.Count;
                    // group on zone_group instead of corridor
                    groupedData = (from row in retVal.AsEnumerable()
                                   group row by new { label = row[1].ToString() } into g
                                   select new AverageDTO
                                   {
                                       label = g.Key.label,
                                       avg = g.Average(x => x[avgColIndex].ToDouble()),
                                       delta = g.Average(x => x[deltaColIndex].ToDouble()),
                                       weight = g.Count() / total
                                   }).ToList();
                }
                else
                {
                    var total = (double)retVal.Rows.Count;
                    groupedData = (from row in retVal.AsEnumerable()
                                   group row by new { label = row[idColIndex].ToString() } into g
                                   select new AverageDTO
                                   {
                                       label = g.Key.label,
                                       avg = g.Average(x => x[avgColIndex].ToDouble()),
                                       delta = g.Average(x => x[deltaColIndex].ToDouble()),
                                       weight = g.Count() / total
                                   }).ToList();
                }
                return groupedData.ToList();
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "metrics/average", ex);
                return null;
            }
        }

        /// <summary>
        /// Legacy API method for getting the PTI metric from the 'All RTOP' group
        /// </summary>
        /// <param name="year">4 digit number for year (ex. 2021)</param>
        /// <param name="quarter">Single digit number for quarter (1, 2, 3, 4)</param>
        /// <returns></returns>
        [HttpGet("rtop/pti")]
        public async Task<DataTable> GetQuarterlyLegacyPTIForAllRTOP(int year, int quarter)
        {
            try
            {
                var metricsData = new MetricsDataAccessLayer();
                var retVal = await metricsData.GetQuarterlyLegacyPTIForAllRTOP(SqlConnectionReader, year, quarter);
                return retVal;
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name, "metrics/rtop/pti", ex);
                return null;
            }
        }

        /// <summary>
        /// API method for returning safety, operation, and maintenance health averages for a given month.
        /// </summary>
        /// <param name="zoneGroup">>Zone Group (aka Signal Group) to pull data for.  null returns all groups</param>
        /// <param name="month">The month to pull data for</param>
        /// <returns>List of averages in order of {operation, maintenance, safety}</returns>
        [HttpGet("monthaverages")]
        public async Task<List<double>> GetAveragesForMonth(string zoneGroup, string month)
        {
            try
            {
                MetricsDataAccessLayer metricsData = new MetricsDataAccessLayer();
                var s = await metricsData.GetAverageForMonth(SqlConnectionReader, "safety", zoneGroup, month);
                var o = await metricsData.GetAverageForMonth(SqlConnectionReader, "ops", zoneGroup, month);
                var m = await metricsData.GetAverageForMonth(SqlConnectionReader, "maint", zoneGroup, month);
                return new List<double> { o, m, s };
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer.WriteToErrorLog(SqlConnectionWriter,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "metrics/monthaverages", ex);
                return new List<double> { -1, -1, -1 };
            }
        }

      

        /// <summary>
        /// Checks if the database is setup to calculate data based on the filter passed in.
        /// TODO: Add additional invalid filters
        /// </summary>
        /// <param name="measure"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private bool IsFilterValid(string measure, string interval)
        {
            switch (measure)
            {
                case "tp":
                    switch (interval)
                    {
                        case "hr":
                        case "qhr":
                            return false;
                    }
                    break;
                case "tti":
                case "pti":
                    switch (interval)
                    {
                        case "mo":
                        case "qu":
                            return true;
                    }
                    return false;
            }
            return true;
        }
    }
}
