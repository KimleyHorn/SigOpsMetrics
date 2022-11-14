using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using MySqlConnector;
using OfficeOpenXml.Export.ToDataTable;
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

        public async Task<DataTable> GetFilteredDataTable(string source, string measure, FilterDTO filter, MySqlConnection sqlConnection, bool signalOnly = false)
        {
            // Check for an invalid filter
            //todo more checks as we start using the filter
            if (filter.timePeriod < 0) return null;

            //Quarterly data is formatted differently
            var interval = GetIntervalFromFilter(filter);
            var dates = GenerateDateFilter(filter);
            string startQuarter = null, endQuarter = null;
            string startDate = string.Empty;
            string endDate = string.Empty;
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
            //Get a list of corridors and all signals belonging to this filter
            IEnumerable<Signal> signalsWithCorridors = await SignalsDataAccessLayer.GetSignalsWithCorridors(sqlConnection, filter);
            
            // New stuff
            if (signalsWithCorridors.Any())
            {
                //If signalOnly = true then force the level as signals otherwise check to see if the filter has a corridor assigned.
                string level = string.IsNullOrWhiteSpace(filter.corridor) ? "cor" : "sig";
                level = signalOnly == true ? "sig" : level;
                bool allZoneGroup = filter.zone_Group == "All" ? true : false;
                var corridors = await GetMetricByFilterWithSignalsAndCorridors(sqlConnection, source, measure, interval, startDate, endDate,
                                                                                   signalsWithCorridors.ToList(), level, allZoneGroup);
                return corridors;
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
                        var yr = quarter == 4 ? dt.Year -1: dt.Year;
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

            return (fullStart, fullEnd);
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

            var interval = GetIntervalFromFilter(filter);

            // After reworking how the signals/corridors are calculated, all indexes are the same except the following.
            if (measure == "maint_plot" || measure == "ops_plot" || measure == "safety_plot")
            {
                idColIndex = 1;
            }

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


        /// <summary>
        /// Returns a datatable with data at either a signal level or corridor level. 
        /// If returning data at a corridor level, we first get all the signal data and group/calculate it up to the corridor levels.
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="source"></param>
        /// <param name="measure"></param>
        /// <param name="interval"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="signals"></param>
        /// <param name="filterLevel"></param>
        /// <param name="all"></param>
        /// <returns></returns>
        public static async Task<DataTable> GetMetricByFilterWithSignalsAndCorridors(MySqlConnection sqlConnection, string source, 
            string measure, string interval, string start, string end, List<Signal> signals, string filterLevel, bool all = false)
        {
            string level = "sig";
            var dateRangeClause = CreateDateRangeClause(interval, measure, start, end);

            //Several table structures do not accomodate for Signals so they have to use Corridors instead.
            List<string> idsForWhereClause = new List<string>();
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
            var results = await GetFromDatabase(sqlConnection, level, interval, measure, fullWhereClause, all);

            //Need to check if we are returning data at a corridor level. If so convert the signals datatable to corridors.
            //Otherwise leave it as the signals table and return the data.
            //TTI does not get calculated/averaged and should just return the data as well.
            if (ReturnDataAtSignalLevel(filterLevel, measure))
            {
                return results;
            }

            //Setup a new datatable
            string intervalColumnName = GetIntervalColumnName(interval);
            string calculatedDataColumnName = GetCalculatedValueColumnName(measure);
            string measureColumnName = String.Empty; // GetMeasureColumnName(measure); // This needs refactoring because the column is not used in this functionality.
            string tableName = ValidateTableName(filterLevel, interval, measure);
            DataTable groupedDataTable = CreateDataTableByLevelAndIntervalAndMeasure(tableName, intervalColumnName, calculatedDataColumnName, measureColumnName);

            //Loop through each corridor
            foreach (string corridorId in signals.Select(c => c.Corridor).Distinct().ToList())
            {
                //Get all signals for this corridor.                
                //cctv data appears to use Corridor data instead of signals so we can try selecting the Corridor here instead of SignalId
                List<string> ids = new List<string>();
                if (measure == "cctv")
                {
                    ids = signals.Where(p => p.Corridor == corridorId).Select(s => s.Corridor).Distinct().ToList();
                }
                else
                {
                    ids = signals.Where(p => p.Corridor == corridorId).Select(s => s.SignalId).Distinct().ToList();
                }

                //Get all datatable rows where the signalId is in the list above.
                //This will get all signals for the corridor being used. (grouped data)
                var dtCorrs = results.AsEnumerable().Where(p => ids.Contains(p.Field<string>("Corridor")));

                if (!dtCorrs.Any())
                {
                    continue;
                }
                //Average the data based on the interval column - Month, Week, etc..
                //If the interval is quarterly we need to average the data differently due to the intervalColumnName "Quarter" is not in a date format so it must be cast each time.
                //I separated that functionalty out so there is not a check on formatting/casting the data every time for other intervals. 
                List<Corridor> averagedData = new List<Corridor>();
                if (interval == "qu")
                {
                    averagedData = GetAverageQuarterIntervalData(dtCorrs, corridorId, intervalColumnName, calculatedDataColumnName, all);
                }
                else
                {
                    averagedData = GetAverageStandardIntervalData(dtCorrs, corridorId, intervalColumnName, calculatedDataColumnName, all);
                }

                foreach (var corridor in averagedData)
                {
                    DataRow dr = groupedDataTable.NewRow();
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

        /// <summary>
        /// Averages signal data up to a corridor level for all intervals except quarter.
        /// </summary>
        /// <param name="corridorData"></param>
        /// <param name="corridorId"></param>
        /// <param name="intervalColumnName"></param>
        /// <param name="calculatedDataColumnName"></param>
        /// <returns></returns>
        private static List<Corridor> GetAverageStandardIntervalData(IEnumerable<DataRow> corridorData, string corridorId, string intervalColumnName, string calculatedDataColumnName, bool allZones)
        {
            //TODO: If using Zones, we need to separate that out as well. Otherwise do not worry about that.
            if (allZones)
            {
                var averagedData = corridorData.GroupBy(x => new
                {
                    zoneGroup = x.Field<string>("Zone_Group"), // ZoneGroup is the Region. There is a specific function for that. Separate this out.
                    intervalColumnName = x.Field<DateTime>(intervalColumnName) // Does this need to be a DateTime or can it stay as a string until converting the data below?
                })
                    .Select(x => new Corridor()
                    {
                        CorridorId = corridorId,
                        TimePeriod = x.Key.intervalColumnName,
                        ZoneGroup = x.Key.zoneGroup,
                        CalculatedField = x.Average(xx => xx.Field<double>(calculatedDataColumnName)),
                        Delta = x.Average(xx => xx.Field<double>("delta")),

                    }).ToList();
                return averagedData;
            }
            else
            {
                var averagedData = corridorData.GroupBy(x => new
                {
                    intervalColumnName = x.Field<DateTime>(intervalColumnName) // Does this need to be a DateTime or can it stay as a string until converting the data below?
                })
                    .Select(x => new Corridor()
                    {
                        CorridorId = corridorId,
                        TimePeriod = x.Key.intervalColumnName,
                        CalculatedField = x.Average(xx => xx.Field<double>(calculatedDataColumnName)),
                        Delta = x.Average(xx => xx.Field<double>("delta")),

                    }).ToList();
                return averagedData;
            }
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
        private static List<Corridor> GetAverageQuarterIntervalData(IEnumerable<DataRow> corridorData, string corridorId, string intervalColumnName, string calculatedDataColumnName, bool allZones)
        {

            //If using Zones, we need to separate that out as well.
            if (allZones)
            {
                var averagedData = corridorData.GroupBy(x => new
                {
                    intervalColumnName = x.Field<string>(intervalColumnName)
                })
                    .Select(x => new Corridor()
                    {
                        CorridorId = corridorId,
                        TimePeriod = x.Key.intervalColumnName.ConvertQuarterStringToDateTime(),
                        CalculatedField = x.Average(xx => xx.Field<double>(calculatedDataColumnName)),
                        Delta = x.Average(xx => xx.Field<double>("delta")),

                    }).ToList();
                return averagedData;
            }
            else
            {
                var averagedData = corridorData.GroupBy(x => new
                {
                    zoneGroup = x.Field<string>("Zone_Group"), // ZoneGroup is the Region. There is a specific function for that. Separate this out.
                    intervalColumnName = x.Field<string>(intervalColumnName)
                })
                    .Select(x => new Corridor()
                    {
                        CorridorId = corridorId,
                        TimePeriod = x.Key.intervalColumnName.ConvertQuarterStringToDateTime(),
                        ZoneGroup = x.Key.zoneGroup,
                        CalculatedField = x.Average(xx => xx.Field<double>(calculatedDataColumnName)),
                        Delta = x.Average(xx => xx.Field<double>("delta")),

                    }).ToList();
                return averagedData;
            }
            
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
            
            dt.Columns.Add("Corridor", typeof(string));
            dt.Columns.Add("Zone_Group", typeof(string));
            //If the interval is quarter we need to format the "Quarter" column differently.
            DataColumn dc = new DataColumn(intervalColumnName, typeof(string));
            if (intervalColumnName == "Quarter")
            {
                dc.MaxLength = 30;
            }
            dt.Columns.Add(dc);
            dt.Columns.Add(calculatedDataColumnName, typeof(string));
            dt.Columns.Add(measureColumnName, typeof(string)); // This column is not used here
            dt.Columns.Add("delta", typeof(string));
            dt.Columns.Add("Description", typeof(string)); // This column is not used here.

            

                return dt;
        }

        /// <summary>
        /// Gets the name of the data column for the interval being used.
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        private static string GetIntervalColumnName(string interval)
        {
            switch(interval)
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
        private static string GetCalculatedValueColumnName(string measure)
        {
            //TODO: Figure out tsou
            switch (measure)
            {
                case "aogd":
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
                case "prd":
                    return "pr";
                case "qsd":
                    return "qs_freq";
                case "reported":
                    return "reported";
                case "resolved":
                    return "resolved";
                case "sfd":
                case "sfo":
                    return "sf_freq";
                case "tp":
                case "vphpa":
                case "vphpp":
                    return "vph";
                case "vpd":
                case "vph":
                    return "vpd";                
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
            cmd.CommandText = $"select * from {AppConfig.DatabaseName}.rtop_pti where year = {year} && quarter = {quarter}";
            cmd.Connection = sqlConnection;

            await using (cmd)
            {
                await using var reader = await cmd.ExecuteReaderAsync();
                tb.Load(reader);
            }

            return tb;
        }

        private static async Task<DataTable> GetFromDatabase(MySqlConnection sqlConnection, string level,
            string interval, string measure,
            string whereClause, bool all = false, MySqlCommand cmd = null)
        {
            var tb = new DataTable();
            if (cmd == null)
            {
                cmd = new MySqlCommand();
            }
            try
            {
                if (all)
                {
                    var tableName = BaseDataAccessLayer.ValidateTableName(level, interval, measure);
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

            // Old way. Converted to a String.Join instead of a foreach loop to speed it up.
            //foreach (var row in itemIDs)
            //{
            //    newWhere += $"'{row}',";
            //}
            //newWhere = newWhere.Substring(0, newWhere.Length - 1); //chop off last comma
            //newWhere += ")";

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