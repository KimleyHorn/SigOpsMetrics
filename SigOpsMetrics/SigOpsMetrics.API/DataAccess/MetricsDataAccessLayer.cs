using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MySqlConnector;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.DTOs;
using SigOpsMetrics.API.Classes.Extensions;
using SigOpsMetrics.API.Classes.Internal;

#pragma warning disable 1591

namespace SigOpsMetrics.API.DataAccess
{
    public class MetricsDataAccessLayer : BaseDataAccessLayer
    {
        private const string ApplicationName = "SigOpsMetrics.API";

        public async Task<Dictionary<string, List<SummaryTrendDTO>>> GetSummaryTrend(string source, FilterDTO filter, MySqlConnection sqlConnectionReader,
            MySqlConnection sqlConnectionWriter)
        {
            var timer = Stopwatch.StartNew();
            //var dt = DateTime.Today;

            //var fullStart = new DateTime(dt.Year - 1, dt.Month, 1);
            //var fullEnd = new DateTime(dt.Year, dt.Month, DateTime.DaysInMonth(dt.Year, dt.Month));

            //Quarterly data is formatted differently
            var interval = GetIntervalFromFilter(filter);
            var dates = GenerateDateFilter(filter);
            string startDate;
            string endDate;
            if (interval == "qu")
            {
                startDate = dates.Item1.NearestQuarterEnd();
                endDate = dates.Item2.NearestQuarterEnd();
            }
            else
            {
                startDate = dates.Item1.ToString();
                endDate = dates.Item2.ToString();
            }

            var allZoneGroup = filter.zone_Group == "All";

            var response = new Dictionary<string, List<SummaryTrendDTO>>();
            if (interval != "hr" && interval != "qhr")
            {
                response = new Dictionary<string, List<SummaryTrendDTO>>
                {
                    { "tp", null },
                    { "aogd", null},
                    { "prd", null},
                    { "qsd", null},
                    { "sfd", null},
                    { "sfo", null},
                    { "tti", null},
                    { "pti", null},
                    { "vpd", null},
                    { "vphpa", null},
                    { "vphpp", null},
                    { "papd", null},
                    { "du", null},
                    { "pau", null},
                    { "cctv", null},
                    { "cu", null}
                };
            }
            else
            {
                response = new Dictionary<string, List<SummaryTrendDTO>>
                {
                    { "tp", null },
                    { "aogh", null},
                    { "prh", null},
                    { "qsh", null},
                    { "sfh", null},
                    { "sfo", null},
                    { "tti", null},
                    { "pti", null},
                    { "vph", null},
                    { "vphpa", null},
                    { "vphpp", null},
                    { "paph", null},
                    { "du", null},
                    { "pau", null},
                    { "cctv", null},
                    { "cu", null}
                };
            }


            var keys = new List<string>(response.Keys);
            var signalsWithCorridors = await SignalsDataAccessLayer.GetSignalsWithCorridors(sqlConnectionReader, filter);
            var cameras = await CamerasDataAccessLayer.GetCameras(sqlConnectionReader, filter);
            // todo after .net 6 upgrade, see if foreachasync is faster
            var tasks = keys.Select(measure => GetSummaryTrendDataAsync(measure, sqlConnectionReader, response,
                startDate, endDate, interval, cameras, signalsWithCorridors, allZoneGroup));
            await Task.WhenAll(tasks);
            timer.Stop();
            var time = timer.Elapsed;
            return response;
        }

        private readonly SemaphoreSlim _sem = new SemaphoreSlim(50);
        private async Task GetSummaryTrendDataAsync(string measure, IDbConnection connection, IDictionary<string, List<SummaryTrendDTO>> response,
            string fullStart, string fullEnd, string interval, IEnumerable<Cctv> cameras, IEnumerable<Signal> signalsWithCorridors, bool allZoneGroup)
        {
            await _sem.WaitAsync();
            try
            {
                // Persist Security Info must be set to true in connection string
                var newConnection = new MySqlConnection(connection.ConnectionString);

                // get average for every month in date range for given filter and add that to dictionary
                var dateRangeWhere = CreateDateRangeClause(interval, measure, fullStart, fullEnd);
                switch (measure)
                {
                    case "cctv":
                        var camerasList = cameras.ToList();
                        if (camerasList.Any())
                        {
                            var ids = camerasList.Select(s => s.CameraId).Distinct().ToList();
                            var fullWhereClause = AddCctvsToWhereClause(dateRangeWhere, ids);
                            var data = await GetFromDatabase(newConnection, "sig", interval, measure, fullWhereClause);
                            var i = GetIntervalColumnName(interval);
                            var dateGroups = data.AsEnumerable().GroupBy(r => r[i]);
                            var avg = new List<SummaryTrendDTO>();
                            if (interval == "qu")
                            {
                                foreach (var date in dateGroups)
                                {
                                    int year = int.Parse(date.Key.ToString().Split('.')[0]);
                                    int quarter = int.Parse(date.Key.ToString().Split('.')[1]);
                                    DateTime quarterStart = new DateTime(year, (quarter - 1) * 3 + 1, 1);
                                    avg.Add(new SummaryTrendDTO
                                    {
                                        Average = date.Average(z => z.Field<double>(GetCalculatedValueColumnName(measure, interval))),
                                        Month = quarterStart
                                    });
                                }
                            }
                            else
                            {
                                avg = dateGroups.Select(x => new SummaryTrendDTO
                                {
                                    Average = x.Average(z => z.Field<double>(GetCalculatedValueColumnName(measure, interval))),
                                    Month = DateTime.Parse(x.Key.ToString())
                                }).ToList();
                            }

                            response[measure] = avg;
                        }
                        else
                        {
                            response[measure] = new List<SummaryTrendDTO>();
                        }

                        break;
                    default:
                        var level = "sig";
                        //Several table structures do not accommodate for Signals so they have to use Corridors instead.
                        var idsForWhereClause = new List<string>();
                        if (UseCorridorForWhereClause(measure))
                        {
                            idsForWhereClause = signalsWithCorridors.Select(s => s.Corridor).Distinct().ToList();
                        }
                        else
                        {
                            idsForWhereClause = signalsWithCorridors.Select(s => s.SignalId).Distinct().ToList();
                        }

                        if (idsForWhereClause.Any())
                        {
                            var fullWhere = AddSignalsToWhereClause(dateRangeWhere, idsForWhereClause, level);
                            //Need to reset the level for Travel Time Index (tti) measures since they are not calculated at a signal level.
                            level = measure != "tti" && measure != "pti" ? "sig" : "cor";

                            // this returns a list of everything from the signal details tables.
                            var results = await GetFromDatabase(newConnection, level, interval, measure, fullWhere,
                                allZoneGroup);
                            var i = GetIntervalColumnName(interval);
                            var dates = results.AsEnumerable().GroupBy(r => r[i]).OrderBy(x => x.Key);

                            var avg = new List<SummaryTrendDTO>();
                            if (interval == "qu")
                            {
                                foreach (var date in dates)
                                {
                                    int year = int.Parse(date.Key.ToString().Split('.')[0]);
                                    int quarter = int.Parse(date.Key.ToString().Split('.')[1]);
                                    DateTime quarterStart = new DateTime(year, (quarter - 1) * 3 + 1, 1);
                                    avg.Add(new SummaryTrendDTO
                                    {
                                        Average = date.Average(z => z.Field<double>(GetCalculatedValueColumnName(measure, interval))),
                                        Month = quarterStart
                                    });
                                }
                            }
                            else
                            {
                                avg = dates.Select(x => new SummaryTrendDTO
                                {
                                    Average = x.Average(z => z.Field<double>(GetCalculatedValueColumnName(measure, interval))),
                                    Month = DateTime.Parse(x.Key.ToString())
                                }).ToList();
                            }

                            response[measure] = avg;
                        }
                        else
                        {
                            response[measure] = new List<SummaryTrendDTO>();
                        }

                        break;
                }
            }
            catch (Exception ex)
            {
                response[measure] = new List<SummaryTrendDTO>();
            }
            finally
            {
                _sem.Release();
            }
        }

        public async Task<DataTable> GetFilteredDataTable(string source, string measure, FilterDTO filter, MySqlConnection sqlConnectionReader,
            MySqlConnection sqlConnectionWriter, bool signalOnly = false)
        {
            // Check for an invalid filter
            //todo more checks as we start using the filter
            if (filter.timePeriod < 0) return null;

            //Quarterly data is formatted differently
            var interval = GetIntervalFromFilter(filter);
            var dates = GenerateDateFilter(filter);
            string startDate;
            string endDate;
            if (interval == "qu")
            {
                startDate = dates.Item1.NearestQuarterEnd();
                endDate = dates.Item2.NearestQuarterEnd();
            }
            else
            {
                startDate = dates.Item1.ToString();
                endDate = dates.Item2.ToString();
            }

            measure = UpdateMeasure(measure, interval);
            var allZoneGroup = filter.zone_Group == "All";
            if (measure == "cctv")
            {
                var cameras = await CamerasDataAccessLayer.GetCameras(sqlConnectionReader, filter);
                var camerasList = cameras.ToList();
                if (camerasList.Any())
                {
                    var dt = await GetCctvMetricByFilter(sqlConnectionReader, sqlConnectionWriter, source, measure, interval, startDate, endDate, camerasList,
                        string.IsNullOrWhiteSpace(filter.corridor), allZoneGroup, signalOnly);
                    return dt;
                }
            }
            else
            {
                //Get a list of corridors and all signals belonging to this filter
                var signalsWithCorridors = await SignalsDataAccessLayer.GetSignalsWithCorridors(sqlConnectionReader, filter);
                // New stuff
                if (signalsWithCorridors.Any())
                {
                    //If signalOnly = true then force the level as signals otherwise check to see if the filter has a corridor assigned.
                    var level = string.IsNullOrWhiteSpace(filter.corridor) ? "cor" : "sig";
                    level = signalOnly ? "sig" : level;

                    var corridors = await GetMetricByFilterWithSignalsAndCorridors(sqlConnectionReader, sqlConnectionWriter, source, measure, interval, startDate, endDate,
                                                                                       signalsWithCorridors.ToList(), level, allZoneGroup, signalOnly);
                    return corridors;
                }
            }

            // Return null if there are no signals to report.
            return null;
        }

        private static string UpdateMeasure(string measure, string interval)
        {
            //Switch from certain daily to hourly tables here
            if (interval != "hr" && interval != "qhr") return measure;
            switch (measure)
            {
                case "aogd":
                    measure = "aogh";
                    break;
                case "papd":
                    measure = "paph";
                    break;
                case "prd":
                    measure = "prh";
                    break;
                case "qsd":
                    measure = "qsh";
                    break;
                case "sfd":
                    measure = "sfh";
                    break;
                case "vpd":
                    measure = "vph";
                    break;
            }

            return measure;
        }

        private static int GetStartMonthForQuarter(int quarter)
        {
            return quarter switch
            {
                1 => 1,
                2 => 4,
                3 => 7,
                4 => 10,
                _ => -1
            };
        }

        private (DateTime, DateTime) GenerateDateFilter(FilterDTO filter)
        {
            var dt = DateTime.Today;
            // If January, set start to December of previous year
            var fullStart = dt.Month == 1 ? new DateTime(dt.Year - 1, 12, 1) : new DateTime(dt.Year, dt.Month - 1, 1);
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
                        var quarter = (int)Math.Ceiling((double)dt.AddMonths(-3).Month / 3);
                        var startMonth = GetStartMonthForQuarter(quarter);
                        var yr = quarter == 4 ? dt.Year - 1 : dt.Year;
                        fullStart = new DateTime(yr, startMonth, 1);
                        fullEnd = new DateTime(yr, startMonth + 2, DateTime.DaysInMonth(yr, startMonth + 2));
                        break;
                    case GenericEnums.DateRangeType.PriorYear:
                        var priorYear = dt.Year - 1;
                        fullStart = new DateTime(priorYear, dt.Month, dt.Day);
                        fullEnd = dt;
                        break;
                    case GenericEnums.DateRangeType.Custom:
                        fullStart = Convert.ToDateTime(filter.customStart);
                        var timeStart = Convert.ToDateTime(filter.startTime);
                        fullStart = fullStart.Date.Add(timeStart.TimeOfDay);

                        fullEnd = Convert.ToDateTime(filter.customEnd);
                        var timeEnd = Convert.ToDateTime(filter.endTime);
                        fullEnd = fullEnd.Date.Add(timeEnd.TimeOfDay);
                        break;
                    case GenericEnums.DateRangeType.PriorMonth:
                    default:
                        int priorMonth;
                        int year;
                        // If January, set month to December of previous year
                        if (dt.Month == 1)
                        {
                            priorMonth = 12;
                            year = dt.Year - 1;
                        }
                        else
                        {
                            priorMonth = dt.Month - 1;
                            year = dt.Year;
                        }
                        fullStart = new DateTime(year, priorMonth, 1);
                        fullEnd = new DateTime(year, priorMonth, DateTime.DaysInMonth(year, priorMonth));
                        break;
                }
            }

            return (fullStart.ToUniversalTime(), fullEnd.ToUniversalTime());
        }

        public string GetIntervalFromFilter(FilterDTO filter)
        {
            var aggregationType = (GenericEnums.DataAggregationType)filter.timePeriod;
            return EnumDescriptions.GetDescriptionFromEnumValue(aggregationType);
        }

        public (int idColIndex, int avgColIndex, int deltaColIndex) GetAvgDeltaIDColumnIndexes(FilterDTO filter, string measure, bool isCorridor)
        {
            return isCorridor
                ? GetCorridorAvgDeltaIDColumnIndexes(filter, measure)
                : GetSignalAvgDeltaIDColumnIndexes(filter, measure);
        }

        private (int idColIndex, int avgColIndex, int deltaColIndex) GetSignalAvgDeltaIDColumnIndexes(FilterDTO filter, string measure)
        {
            var avgColIndex = 2;
            var deltaColIndex = 3;
            var idColIndex = 0;

            var interval = GetIntervalFromFilter(filter);
            if (interval == "hr")
            {
                switch (measure)
                {
                    case "vpd":
                        idColIndex = 0;
                        avgColIndex = 3;
                        deltaColIndex = 4;
                        break;
                }
            }
            if (interval == "wk" || interval == "dy" || interval == "qhr")
            {
                idColIndex = 1;
                avgColIndex = 3;
                deltaColIndex = 4;
                if (measure == "cctv")
                {
                    avgColIndex = 4;
                    deltaColIndex = 5;
                }
            }
            else if (interval == "qu")
            {
                idColIndex = 0;
                avgColIndex = 3;
                deltaColIndex = 5;
            }
            else //mo 
            {
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
                        idColIndex = 1;
                        avgColIndex = 3;
                        deltaColIndex = 4;
                        break;
                    case "du":
                        idColIndex = 1;
                        avgColIndex = 3;
                        deltaColIndex = 6;
                        break;
                    case "cctv":
                    case "maint_plot":
                    case "ops_plot":
                    case "safety_plot":
                        idColIndex = 1;
                        avgColIndex = 4;
                        deltaColIndex = 5;
                        break;
                }
            }

            return (idColIndex, avgColIndex, deltaColIndex);
        }

        private (int idColIndex, int avgColIndex, int deltaColIndex) GetCorridorAvgDeltaIDColumnIndexes(FilterDTO filter, string measure)
        {
            var idColIndex = 0;
            var avgColIndex = 3;
            var deltaColIndex = 5;

            return (idColIndex, avgColIndex, deltaColIndex);
        }

        public static async Task<DataTable> GetMetric(MySqlConnection sqlConnection, string source, string level,
            string interval, string measure, DateTime start, DateTime end)
        {
            var whereClause = CreateDateRangeClause(interval, measure, start.ToString(), end.ToString());
            if (whereClause == string.Empty)
                return new DataTable();

            return await GetFromDatabase(sqlConnection, level, interval, measure, whereClause);
        }

        public static async Task<DataTable> GetMetricByZoneGroup(MySqlConnection sqlConnection, string source, string level,
            string interval, string measure, DateTime start, DateTime end, string zoneGroup)
        {
            MySqlCommand cmd = new MySqlCommand();
            var whereClause = CreateDateRangeAndZoneGroupClause(interval, measure, start, end, zoneGroup, cmd);
            if (whereClause == string.Empty)
                return new DataTable();

            return await GetFromDatabase(sqlConnection, level, interval, measure, whereClause);
        }

        public static async Task<DataTable> GetMetricByCorridor(MySqlConnection sqlConnection, string source, string level,
            string interval, string measure, DateTime start, DateTime end, string corridor)
        {
            MySqlCommand cmd = new MySqlCommand();
            var whereClause = CreateDateRangeAndCorridorClause(interval, measure, start, end, corridor, cmd);
            if (whereClause == string.Empty)
                return new DataTable();

            return await GetFromDatabase(sqlConnection, level, interval, measure, whereClause);
        }

        public static async Task<DataTable> GetCctvMetricByFilter(MySqlConnection reader, MySqlConnection writer, string source,
            string measure, string interval, string start, string end, List<Cctv> cameras, bool groupByCorridor, bool all = false, bool signalOnly = false)
        {
            try
            {
                var dateRangeClause = CreateDateRangeClause(interval, measure, start, end);
                var idsForWhereClause = cameras.Select(s => s.CameraId).Distinct().ToList();
                var fullWhereClause = AddCctvsToWhereClause(dateRangeClause, idsForWhereClause);
                var data = await GetFromDatabase(reader, "sig", interval, measure, fullWhereClause);
                if (!groupByCorridor || signalOnly)
                    return data;

                // group by corridor
                //Setup a new datatable
                var intervalColumnName = GetIntervalColumnName(interval);
                var calculatedDataColumnName = GetCalculatedValueColumnName(measure, interval);
                var measureColumnName = string.Empty; // GetMeasureColumnName(measure); // This needs refactoring because the column is not used in this functionality.
                var tableName = ValidateTableName("cor", interval, measure);
                var groupedDataTable = CreateDataTableByLevelAndIntervalAndMeasure(tableName, intervalColumnName, calculatedDataColumnName, measureColumnName);

                if (all)
                {
                    foreach (var zone in cameras.Select(z => z.ZoneGroup).Distinct().ToList())
                    {
                        // todo ramp meters?
                        if (zone == null)
                        {
                            continue;
                        }

                        var ids = cameras.Where(x => x.ZoneGroup == zone).Select(z => z.CameraId).Distinct().ToList();
                        var dtCorrs = data.AsEnumerable().Where(p => ids.Contains(p.Field<string>("Corridor")));

                        if (!dtCorrs.Any())
                        {
                            continue;
                        }

                        var averagedData = GetAverageForAllZones(dtCorrs, zone, intervalColumnName, calculatedDataColumnName, interval == "qu",
                            writer);

                        foreach (var corridor in averagedData)
                        {
                            var dr = groupedDataTable.NewRow();
                            dr["Corridor"] = corridor.CorridorId;
                            dr["Zone_Group"] = corridor.ZoneGroup;
                            dr[intervalColumnName] = corridor.TimePeriod;
                            dr[calculatedDataColumnName] = corridor.CalculatedField;
                            dr["delta"] = corridor.Delta;
                            dr["ActualZoneGroup"] = corridor.ZoneGroup;
                            dr["Description"] = corridor.ZoneGroup;

                            groupedDataTable.Rows.Add(dr);
                        }
                    }

                    return groupedDataTable;
                }

                //Loop through each corridor
                foreach (var corridorId in cameras.Select(c => c.Corridor).Distinct().ToList())
                {
                    var ids = cameras.Where(p => p.Corridor == corridorId).Select(s => s.Corridor).Distinct().ToList();

                    //Get all datatable rows where the signalId is in the list above.
                    //This will get all signals for the corridor being used. (grouped data)
                    var dtCorrs = data.AsEnumerable().Where(p => ids.Contains(p.Field<string>("Zone_Group")));

                    if (!dtCorrs.Any())
                    {
                        continue;
                    }
                    //Average the data based on the interval column - Month, Week, etc..
                    //If the interval is quarterly we need to average the data differently due to the intervalColumnName "Quarter" is not in a date format so it must be cast each time.
                    //I separated that functionality out so there is not a check on formatting/casting the data every time for other intervals. 
                    List<Corridor> averagedData;
                    if (interval == "qu")
                    {
                        averagedData = GetAverageQuarterIntervalData(dtCorrs, corridorId, intervalColumnName, calculatedDataColumnName);
                    }
                    else
                    {
                        averagedData = GetAverageStandardIntervalData(dtCorrs, corridorId, intervalColumnName, calculatedDataColumnName, writer);
                    }

                    foreach (var corridor in averagedData)
                    {
                        var dr = groupedDataTable.NewRow();
                        dr["Corridor"] = corridor.CorridorId;
                        dr["Zone_Group"] = corridor.ZoneGroup;
                        dr[intervalColumnName] = corridor.TimePeriod;
                        dr[calculatedDataColumnName] = corridor.CalculatedField;
                        dr["delta"] = corridor.Delta;

                        groupedDataTable.Rows.Add(dr);
                    }
                }
                return groupedDataTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _ = WriteToErrorLog(writer, System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name,
        "GetCctvMetricByFilter", ex);
                throw;
            }
        }

        /// <summary>
        /// Returns a datatable with data at either a signal level or corridor level. 
        /// If returning data at a corridor level, we first get all the signal data and group/calculate it up to the corridor levels.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="source"></param>
        /// <param name="measure"></param>
        /// <param name="interval"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="signals"></param>
        /// <param name="filterLevel"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public static async Task<DataTable> GetMetricByFilterWithSignalsAndCorridors(MySqlConnection reader, MySqlConnection writer, string source,
        string measure, string interval, string start, string end, List<Signal> signals, string filterLevel, bool all = false, bool signalsOnly = false)
        {
            var level = "sig";
            var dateRangeClause = CreateDateRangeClause(interval, measure, start, end);

            //Several table structures do not accommodate for Signals so they have to use Corridors instead.
            var idsForWhereClause = new List<string>();
            if (UseCorridorForWhereClause(measure))
            {
                idsForWhereClause = signals.Select(s => s.Corridor).Distinct().ToList();
            }
            else
            {
                idsForWhereClause = signals.Select(s => s.SignalId).Distinct().ToList();
            }

            var fullWhereClause = AddSignalsToWhereClause(dateRangeClause, idsForWhereClause, level);

            //Need to reset the level for Travel Time Index (tti) measures since they are not calculated at a signal level.
            level = measure != "tti" && measure != "pti" ? "sig" : "cor";

            // this returns a list of everything from the signal details tables.
            var results = await GetFromDatabase(reader, level, interval, measure, fullWhereClause, all);

            //Need to check if we are returning data at a corridor level. If so convert the signals datatable to corridors.
            //Otherwise leave it as the signals table and return the data.
            //TTI does not get calculated/averaged and should just return the data as well.
            if (ReturnDataAtSignalLevel(filterLevel, measure) || signalsOnly)
            {
                return results;
            }

            //Setup a new datatable
            var intervalColumnName = GetIntervalColumnName(interval);
            var calculatedDataColumnName = GetCalculatedValueColumnName(measure, interval);
            var measureColumnName = string.Empty; // GetMeasureColumnName(measure); // This needs refactoring because the column is not used in this functionality.
            var tableName = ValidateTableName(filterLevel, interval, measure);
            var groupedDataTable = CreateDataTableByLevelAndIntervalAndMeasure(tableName, intervalColumnName, calculatedDataColumnName, measureColumnName);

            // All Zones filter should group up to the region/zone instead of corridor.
            if (all)
            {
                foreach (var zone in signals.Select(p => p.ZoneGroup).Distinct().ToList())
                {
                    var dtCorrs = results.AsEnumerable().Where(p => p.Field<string>("ActualZoneGroup") == zone);

                    if (!dtCorrs.Any())
                    {
                        continue;
                    }

                    var averagedData = GetAverageForAllZones(dtCorrs, zone, intervalColumnName, calculatedDataColumnName, interval == "qu",
                        writer);

                    foreach (var corridor in averagedData)
                    {
                        var dr = groupedDataTable.NewRow();
                        dr["Corridor"] = corridor.CorridorId;
                        dr["Zone_Group"] = corridor.ZoneGroup;
                        dr[intervalColumnName] = corridor.TimePeriod;
                        dr[calculatedDataColumnName] = corridor.CalculatedField;
                        dr["delta"] = corridor.Delta;
                        dr["ActualZoneGroup"] = corridor.ZoneGroup;
                        dr["Description"] = corridor.ZoneGroup;

                        groupedDataTable.Rows.Add(dr);
                    }
                }
                return groupedDataTable;
            }

            //Loop through each corridor
            foreach (var corridorId in signals.Select(c => c.Corridor).Distinct().ToList())
            {
                //Get all signals for this corridor.
                var ids = signals.Where(p => p.Corridor == corridorId).Select(s => s.SignalId).Distinct().ToList();

                //Get all datatable rows where the signalId is in the list above.
                //This will get all signals for the corridor being used. (grouped data)
                var dtCorrs = results.AsEnumerable().Where(p => ids.Contains(p.Field<string>("Corridor")));

                if (!dtCorrs.Any())
                {
                    continue;
                }
                //Average the data based on the interval column - Month, Week, etc..
                //If the interval is quarterly we need to average the data differently due to the intervalColumnName "Quarter" is not in a date format so it must be cast each time.
                //I separated that functionality out so there is not a check on formatting/casting the data every time for other intervals. 
                List<Corridor> averagedData;
                if (interval == "qu")
                {
                    averagedData = GetAverageQuarterIntervalData(dtCorrs, corridorId, intervalColumnName, calculatedDataColumnName);
                }
                else
                {
                    averagedData = GetAverageStandardIntervalData(dtCorrs, corridorId, intervalColumnName, calculatedDataColumnName, writer);
                }

                foreach (var corridor in averagedData)
                {
                    var dr = groupedDataTable.NewRow();
                    dr["Corridor"] = corridor.CorridorId;
                    dr["Zone_Group"] = corridor.ZoneGroup;
                    dr[intervalColumnName] = corridor.TimePeriod;
                    dr[calculatedDataColumnName] = corridor.CalculatedField;
                    dr["delta"] = corridor.Delta;

                    groupedDataTable.Rows.Add(dr);
                }
            }
            return groupedDataTable;
        }

        private static List<Corridor> GetAverageForAllZones(IEnumerable<DataRow> corridorData, string zone, string intervalColumnName, string calculatedDataColumnName, bool isQuarterInterval,
            MySqlConnection writer)
        {
            List<Corridor> averagedData = null;
            try
            {
                var deltaColumn = corridorData.FirstOrDefault().Table.Columns["delta"];
                var deltaStr = deltaColumn != null ? deltaColumn.ColumnName : "up";

                if (isQuarterInterval)
                {
                    averagedData = corridorData.GroupBy(x => new
                    {
                        intervalColumnName = x.Field<string>(intervalColumnName)
                    })
                    .Select(x => new Corridor
                    {
                        CorridorId = zone,
                        TimePeriod = x.Key.intervalColumnName.ConvertQuarterStringToDateTime(),
                        ZoneGroup = zone,
                        CalculatedField = x.Average(xx => !xx.IsNull(calculatedDataColumnName) ? xx.Field<double>(calculatedDataColumnName) : 0),
                        Delta = x.Average(xx => !xx.IsNull(deltaStr) ? xx.Field<double>(deltaStr) : 0),

                    }).OrderBy(m => m.TimePeriod).ToList();
                }
                else
                {
                    averagedData = corridorData.GroupBy(x => new
                    {
                        intervalColumnName = x.Field<DateTime>(intervalColumnName)
                    })
                    .Select(x => new Corridor
                    {
                        CorridorId = zone,
                        TimePeriod = x.Key.intervalColumnName,
                        ZoneGroup = zone,
                        CalculatedField = x.Average(xx => !xx.IsNull(calculatedDataColumnName) ? xx.Field<double>(calculatedDataColumnName) : 0),
                        Delta = x.Average(xx => !xx.IsNull(deltaStr) ? xx.Field<double>(deltaStr) : 0),
                        Description = zone
                    }).OrderBy(m => m.TimePeriod).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _ = WriteToErrorLog(writer, System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name,
                    "GetAverageForAllZones", ex);
            }

            return averagedData;
        }

        /// <summary>
        /// Averages signal data up to a corridor level for all intervals except quarter.
        /// </summary>
        /// <param name="corridorData"></param>
        /// <param name="corridorId"></param>
        /// <param name="intervalColumnName"></param>
        /// <param name="calculatedDataColumnName"></param>
        /// <returns></returns>
        private static List<Corridor> GetAverageStandardIntervalData(IEnumerable<DataRow> corridorData, string corridorId, string intervalColumnName, string calculatedDataColumnName, MySqlConnection writer)
        {
            var averagedData = new List<Corridor>();
            try
            {
                var deltaColumn = corridorData.FirstOrDefault().Table.Columns["delta"];
                var upColumn = corridorData.FirstOrDefault().Table.Columns["up"];
                var deltaStr = deltaColumn != null ? deltaColumn.ColumnName : "up";

                averagedData = corridorData.GroupBy(x => new
                {
                    intervalColumnName = x.Field<DateTime>(intervalColumnName)
                })
                .Select(x => new Corridor()
                {
                    CorridorId = corridorId,
                    TimePeriod = x.Key.intervalColumnName,
                    CalculatedField = x.Average(xx => !xx.IsNull(calculatedDataColumnName) ? xx.Field<double>(calculatedDataColumnName) : 0),
                    Delta = deltaColumn == null && upColumn == null ? 0 : x.Average(xx => !xx.IsNull(deltaStr) ? xx.Field<double>(deltaStr) : 0)

                }).OrderBy(m => m.TimePeriod).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _ = WriteToErrorLog(writer, System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name,
    "GetAverageStandardIntervalData", ex);
            }
            return averagedData;
        }

        /// <summary>
        /// Averages signal data up to a corridor level for the quarter interval filter.
        /// Quarter intervals need to be averaged separately because the date field in the database is a string instead of a DateTime like the other tables.
        /// </summary>
        /// <param name="corridorData"></param>
        /// <param name="corridorId"></param>
        /// <param name="intervalColumnName"></param>
        /// <param name="calculatedDataColumnName"></param>
        /// <returns></returns>
        private static List<Corridor> GetAverageQuarterIntervalData(IEnumerable<DataRow> corridorData, string corridorId, string intervalColumnName, string calculatedDataColumnName)
        {
            var averagedData = corridorData.GroupBy(x => new
            {
                intervalColumnName = x.Field<string>(intervalColumnName)
            })
                .Select(x => new Corridor()
                {
                    CorridorId = corridorId,
                    TimePeriod = x.Key.intervalColumnName.ConvertQuarterStringToDateTime(),
                    CalculatedField = x.Average(xx => !xx.IsNull(calculatedDataColumnName) ? xx.Field<double>(calculatedDataColumnName) : 0),
                    Delta = x.Average(xx => !xx.IsNull("delta") ? xx.Field<double>("delta") : 0),

                }).OrderBy(m => m.TimePeriod).ToList();
            return averagedData;
        }

        /// <summary>
        /// Create a generic datatable used for the calculating and grouping of signals into their correlating corridors
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="intervalColumnName"></param>
        /// <param name="calculatedDataColumnName"></param>
        /// <param name="measureColumnName"></param>
        /// <returns></returns>
        private static DataTable CreateDataTableByLevelAndIntervalAndMeasure(string tableName, string intervalColumnName, string calculatedDataColumnName, string measureColumnName)
        {
            DataTable dt = new DataTable(tableName);

            //Each datatable will have the Corridor, Zone_Group and Delta columns being used
            //The ones column is different depending on measure I believe.
            //Not sure if the Description column is used for this.
            //Depending on the interval and measure, they will have some sort of Date (Month, Week etc..) and something to calculate (vph, pd etc..)
            //If we keep these in the same position then we can easily update them regardless of interval and measure
            //This order is based off the cor_mo_tp structure

            dt.Columns.Add("Corridor", typeof(string));                                         //0
            dt.Columns.Add("Zone_Group", typeof(string));                                       //1
                                                                                                //If the interval is quarter we need to format the "Quarter" column differently.
            DataColumn dc = new DataColumn(intervalColumnName, typeof(string));
            if (intervalColumnName == "Quarter")
            {
                dc.MaxLength = 30;
            }
            dt.Columns.Add(dc);                                                                 //2
            dt.Columns.Add(calculatedDataColumnName, typeof(string));                           //3
            dt.Columns.Add(measureColumnName, typeof(string)); // This column is not used here  //4
            dt.Columns.Add("delta", typeof(string));                                            //5
            dt.Columns.Add("Description", typeof(string)); // This column is not used here.     //6
            dt.Columns.Add("ActualZoneGroup", typeof(string));                                  //7


            return dt;
        }

        /// <summary>
        /// Gets the name of the data column for the interval being used.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        private static string GetIntervalColumnName(string interval)
        {
            switch (interval)
            {
                case "qu":
                    return "Quarter";
                case "mo":
                    return "Month";
                case "wk": // Week is same as Day
                case "dy":
                    return "Date";
                case "hr":
                    return "Hour";
                case "qhr":
                    return "Timeperiod";
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the name of the data column that needs to be calculated.
        /// </summary>
        /// <param name="measure"></param>
        /// <returns></returns>
        private static string GetCalculatedValueColumnName(string measure, string interval)
        {
            //TODO: Figure out tsou
            switch (measure)
            {
                case "aogd":
                case "aogh":
                    return "aog";
                case "cctv":
                case "cu":
                case "du":
                case "pau":
                    return "uptime";
                case "outstanding":
                    return "outstanding";
                case "over45":
                    return "over45";
                case "papd":
                    return "papd";
                case "paph":
                    if (interval == "qhr")
                        return "vol";
                    return "paph";
                case "prd":
                case "prh":
                    return "pr";
                case "qsd":
                case "qsh":
                    return "qs_freq";
                case "reported":
                    return "reported";
                case "resolved":
                    return "resolved";
                case "sfd":
                case "sfo":
                case "sfh":
                    return "sf_freq";
                case "tp":
                case "vphpa":
                case "vphpp":
                    return "vph";
                case "vpd":
                case "vph":
                    return "vpd";
                case "maint_plot":
                case "ops_plot":
                case "safety_plot":
                    return "Percent Health";
                case "tti":
                    return "tti";
                case "pti":
                    return "pti";
                case "":
                    return "";
            }
            return string.Empty;
        }

        /// <summary>
        /// This needs to be refactured because I am not sure how this column is used.
        /// </summary>
        /// <param name="measure"></param>
        /// <returns></returns>
        private static string GetMeasureColumnName(string measure)
        {
            switch (measure)
            {
                case "tp":
                    return "ones";
            }
            return string.Empty;
        }

        /// <summary>
        /// Old method of getting metric data by either corridor or signals.
        /// This was replaced by GetMetricByFilterWithSignalsAndCorridors to always get signals then bubble up to corridors if needed.
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="source"></param>
        /// <param name="measure"></param>
        /// <param name="interval"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="filteredItems"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public static async Task<DataTable> GetMetricByFilter(MySqlConnection sqlConnection, string source, string measure,
            string interval, string start, string end, FilteredItems filteredItems, bool all = false)
        {
            //string level = filteredItems.FilterType == GenericEnums.FilteredItemType.Corridor ? "cor" : "sig";
            string level = "sig";
            var dateRangeClause = CreateDateRangeClause(interval, measure, start, end);
            var fullWhereClause = AddSignalsToWhereClause(dateRangeClause, filteredItems.Items, level);

            var results = await GetFromDatabase(sqlConnection, level, interval, measure, fullWhereClause, all);

            return results;
        }

        public async Task<DataTable> GetQuarterlyLegacyPTIForAllRTOP(MySqlConnection sqlConnection, int year, int quarter)
        {
            var tb = new DataTable();
            var cmd = new MySqlCommand();

            await sqlConnection.OpenAsync();
            cmd.CommandText = "select * from rtop_pti rp where 1=0";
            cmd.Connection = sqlConnection;

            await using (cmd)
            {
                await using var reader = await cmd.ExecuteReaderAsync();
                tb.Load(reader);
            }

            const string url = "http://sigopsmetrics.com:8001/query?source=beta&level=cor&interval=qu&measure=pti";
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                using var ms = new StreamReader(await response.Content.ReadAsStreamAsync());
                while (!ms.EndOfStream)
                {
                    var line = await ms.ReadLineAsync();
                    if (line != null)
                    {
                        var arr = line.Split(',');
                        if (arr[0] == "All RTOP" && arr[2] == $"{year}.{quarter}")
                        {
                            var pti = Math.Round(Convert.ToDecimal(arr[3]), 2);
                            var delta = Math.Round(Convert.ToDecimal(arr[5]), 3);
                            tb.Rows.Add(arr[0], year, quarter, pti, delta);
                            break;
                        }
                    }
                }
            }

            return tb;
        }

        private static async Task<DataTable> GetFromDatabase(MySqlConnection sqlConnection, string level,
            string interval, string measure,
            string whereClause, bool all = false, MySqlCommand cmd = null)
        {
            var tb = new DataTable();
            cmd ??= new MySqlCommand();
            try
            {
                if (all)
                {
                    var tableName = ValidateTableName(level, interval, measure);
                    var type = level == "sig" ? "SignalId" : "Corridor";
                    await sqlConnection.OpenAsync();
                    await using (cmd)
                    {
                        cmd.CommandText = $"select t.*, CASE WHEN s.Zone_Group IS NULL THEN t.Corridor ELSE s.Zone_Group END AS ActualZoneGroup from {AppConfig.DatabaseName}.{tableName} t left join (select {type} AS SignalType, Zone_Group FROM {AppConfig.DatabaseName}.signals GROUP BY {type}, Zone_Group) s ON s.SignalType = t.Corridor {whereClause}";
                        cmd.Connection = sqlConnection;
                        await using var reader = await cmd.ExecuteReaderAsync();
                        tb.Load(reader);
                    }
                }
                else
                {
                    await sqlConnection.OpenAsync();
                    await using (cmd)
                    {
                        cmd.CommandText = CreateSQLStatement(level, interval, measure, whereClause);
                        cmd.Connection = sqlConnection;
                        await using var reader = await cmd.ExecuteReaderAsync();
                        tb.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                await WriteToErrorLog(sqlConnection, System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    nameof(GetFromDatabase), ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return tb;
        }

        public async Task<double> GetAverageForMonth(MySqlConnection connection, string measure, string zoneGroup, string month)
        {
            try
            {
                month = DateTime.Parse(month).ToString("yyyy-MM-dd");
                var where = $"where `Zone Group` = '{zoneGroup}' and `Month` = '{month}'";
                if (zoneGroup == null)
                {
                    where = $"where `Month` = '{month}'";
                }
                var dt = await GetFromDatabase(connection, "cor", "mo", measure, where);
                return (double)dt.Compute("AVG([Percent Health])", "");
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        private static string CreateSQLStatement(string level, string interval, string measure, string whereClause)
        {
            var tableName = BaseDataAccessLayer.ValidateTableName(level, interval, measure);
            switch (interval)
            {
                case "mo":
                    switch (measure)
                    {
                        case "tsub":
                            return
                                $"select Zone_Group, Corridor, Task_Subtype, Month, Reported, Resolved, Outstanding from {AppConfig.DatabaseName}.{tableName} {whereClause}";
                        case "tsou":
                            return
                                $"select Zone_Group, Corridor, Task_Source, Month, Reported, Resolved, Outstanding from {AppConfig.DatabaseName}.{tableName} {whereClause}";
                        case "ttyp":
                            return
                                $"select Zone_Group, Corridor, Task_Type, Month, Reported, Resolved, Outstanding from {AppConfig.DatabaseName}.{tableName} {whereClause}";
                    }
                    break;
                case "hr":
                    switch (measure)
                    {
                        case "vph":
                            return
                                $"select Corridor, Zone_Group, Hour, vph as vpd, delta, Description from {AppConfig.DatabaseName}.{tableName} {whereClause}";
                    }

                    break;
                case "qhr":
                    switch (measure)
                    {
                        case "vph":
                            return
                                $"select Corridor, Zone_Group, Timeperiod, vol as vpd, delta, Description from {AppConfig.DatabaseName}.{tableName} {whereClause}";
                    }

                    break;
            }

            return $"select * from {AppConfig.DatabaseName}.{tableName} {whereClause}";

        }

        private static string CreateDateRangeClause(string interval, string measure, string start, string end)
        {
            string period;
            var startFormat = start;
            var endFormat = end;

            switch (interval)
            {
                case "qhr":
                    period = "timeperiod";
                    startFormat = start.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss");
                    endFormat = end.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                case "hr":
                    period = "hour";
                    startFormat = start.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss");
                    endFormat = end.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                case "dy":
                    period = "date";
                    startFormat = start.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd");
                    endFormat = end.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd");
                    break;
                case "wk":
                    period = "date";
                    startFormat = start.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd");
                    endFormat = end.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd");
                    break;
                case "mo":
                    if (measure == "aogh") //todo more hourly measures?
                    {
                        period = "hour";
                        startFormat = start.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd");
                        endFormat = end.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        period = "month";
                        startFormat = start.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd");
                        endFormat = end.ToNullableDateTime().GetValueOrDefault().ToString("yyyy-MM-dd");
                    }
                    break;
                case "qu":
                    period = "quarter";
                    break;
                default:
                    return string.Empty;
            }

            return $"where {period} >= '{startFormat}' and {period} <= '{endFormat}'";
        }

        private static string CreateZoneGroupAndClause(string zoneGroup, MySqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("Zone_Group", zoneGroup);
            return zoneGroup == "All" ? "" : $" and Zone_Group = @Zone_Group";
        }

        private static string CreateCorridorAndClause(string corridor, MySqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("Corridor", corridor);
            return $" and Corridor = @Corridor";
        }

        private static string CreateDateRangeAndZoneGroupClause(string level, string measure, DateTime start, DateTime end, string zoneGroup, MySqlCommand cmd)
        {
            var dateRangeClause = CreateDateRangeClause(level, measure, start.ToString(), end.ToString());
            if (dateRangeClause == string.Empty)
                return string.Empty;

            return dateRangeClause + CreateZoneGroupAndClause(zoneGroup, cmd);
        }

        private static string CreateDateRangeAndCorridorClause(string level, string measure, DateTime start, DateTime end,
            string corridor, MySqlCommand cmd)
        {
            var dateRangeClause = CreateDateRangeClause(level, measure, start.ToString(), end.ToString());
            if (dateRangeClause == string.Empty)
                return string.Empty;

            return dateRangeClause + CreateCorridorAndClause(corridor, cmd);
        }

        private static string AddCctvsToWhereClause(string whereClause, List<string> itemIDs)
        {
            var newWhere = string.IsNullOrEmpty(whereClause) ? " where " : whereClause;
            newWhere += string.IsNullOrEmpty(whereClause) ? " Corridor in (" : " and Corridor in (";

            if (itemIDs.Any())
            {
                const string separator = "','";
                newWhere += $"'{string.Join(separator, itemIDs)}')";
            }

            return newWhere;
        }

        private static string AddSignalsToWhereClause(string whereClause, List<string> itemIDs, string level)
        {
            var newWhere = string.IsNullOrEmpty(whereClause) ? " where " : whereClause;

            switch (level)
            {
                case "sig":
                    // This will return the "Corridor" from the sig_mo_tp table which is actually links to the signals.SignalId field. 
                    newWhere += string.IsNullOrEmpty(whereClause) ? " Corridor in (" : " and Corridor in (";
                    break;
                case "cor":
                    // This will return the actual Corridor name from the signals table.
                    newWhere += string.IsNullOrEmpty(whereClause) ? " signals.Corridor in (" : " and signals.Corridor in (";
                    break;
            }

            if (itemIDs.Any())
            {
                string separator = "','";
                newWhere += $"'{String.Join(separator, itemIDs)}')";
            }

            return newWhere;
        }

        private static string SetLevelByMeasure(string measure)
        {
            switch (measure)
            {
                case "tti":
                case "pti":
                    return "cor";

            }
            return "sig";
        }

        private static bool UseCorridorForWhereClause(string measure)
        {
            if (measure == "tti" || measure == "pti" || measure == "cctv" || measure == "reported" || measure == "outstanding" || measure == "over45"
                || measure == "tsou" || measure == "resolved" || measure == "ttyp" || measure == "tsub")
            {
                return true;
            }
            return false;
        }

        private static bool ReturnDataAtSignalLevel(string filterLevel, string measure)
        {
            if (filterLevel == "sig" || measure == "tti" || measure == "pti" || measure == "reported" || measure == "outstanding" || measure == "over45"
                || measure == "tsou" || measure == "resolved" || measure == "ttyp" || measure == "tsub")
            {
                return true;
            }
            return false;
        }
    }
}