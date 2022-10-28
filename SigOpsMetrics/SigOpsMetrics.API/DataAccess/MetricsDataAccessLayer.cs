using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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

        public async Task<DataTable> GetFilteredDataTable(string source, string measure, FilterDTO filter, MySqlConnection sqlConnection, bool signalOnly = false)
        {
            // Check for an invalid filter
            //todo more checks as we start using the filter
            if (filter.timePeriod < 0) return null;

            if (!string.IsNullOrEmpty(filter.signalId))
            {
                signalOnly = true;
            }

            //Quarterly data is formatted differently
            var interval = GetIntervalFromFilter(filter);
            var dates = GenerateDateFilter(filter);
            string startQuarter = null, endQuarter = null;
            if (interval == "qu")
            {
                startQuarter = dates.Item1.NearestQuarterEnd();
                endQuarter = dates.Item2.NearestQuarterEnd();
            }

            measure = UpdateMeasure(measure, interval);

            var filteredItems = new FilteredItems();
            if (signalOnly)
            {
                filteredItems = await SignalsDataAccessLayer.GetSignalsByFilter(sqlConnection, filter);
            }
            else
            {
                filteredItems = await SignalsDataAccessLayer.GetCorridorsOrSignalsByFilter(sqlConnection, filter);
            }

            //If we got no corridors/signals, bail
            if (filteredItems.Items.Any())
            {
                if (interval == "qu" && startQuarter != null && endQuarter != null)
                {
                    var retVal = await MetricsDataAccessLayer.GetMetricByFilter(sqlConnection, source, measure, interval,
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
                else if (filter.zone_Group == "All")
                {
                    var retVal =
                        MetricsDataAccessLayer.GetMetricByFilter(sqlConnection, source, measure, interval, dates.Item1.ToString(),
                            dates.Item2.ToString(), filteredItems, true);
                    return await retVal;
                }
                else
                {
                    var retVal =
                        MetricsDataAccessLayer.GetMetricByFilter(sqlConnection, source, measure, interval, dates.Item1.ToString(),
                            dates.Item2.ToString(), filteredItems);
                    return await retVal;
                }

            }

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

        private string GetIntervalFromFilter(FilterDTO filter)
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

            if (interval == "hr" || interval == "qhr")
            {
                if (measure == "vpd")
                {
                    idColIndex = 0;
                    avgColIndex = 3;
                    deltaColIndex = 5;
                }
            }
            else
            {
                if (interval == "dy" && measure == "vpd")
                {
                    avgColIndex = 3;
                    deltaColIndex = 4;
                }
                if (measure == "vphpa" || measure == "vphpp")
                {
                    avgColIndex = 3;
                    deltaColIndex = 4;
                }
                else if (measure == "maint_plot" || measure == "ops_plot" || measure == "safety_plot")
                {
                    idColIndex = 1;
                    avgColIndex = 3;
                    deltaColIndex = 5; //doesn't exist
                }
                else if (measure == "du")
                {
                    avgColIndex = 5;
                    deltaColIndex = 6;
                }
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

        public static async Task<DataTable> GetMetricByFilter(MySqlConnection sqlConnection, string source, string measure,
            string interval, string start, string end, FilteredItems filteredItems, bool all = false)
        {
            string level = filteredItems.FilterType == GenericEnums.FilteredItemType.Corridor ? "cor" : "sig";
            var dateRangeClause = CreateDateRangeClause(interval, measure, start, end);
            var fullWhereClause = AddSignalsToWhereClause(dateRangeClause, filteredItems.Items, level);

            return await GetFromDatabase(sqlConnection, level, interval, measure, fullWhereClause, all);
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
                        case "tp":
                            switch (level)
                            {
                                //If we are at the corridor level, we still need to grab the signals from the sig_{interval}_{measure} table so we can pickup any signals that might have changed corridors.
                                case "cor":
                                    return
                                        $"SELECT signals.Corridor, sig_mo_tp.Zone_group, Month, vph, signals.SignalId, delta, Description FROM {AppConfig.DatabaseName}.sig_mo_tp LEFT OUTER JOIN signals ON sig_mo_tp.Corridor = signals.SignalId {whereClause}";
                            }
                            break;
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

            foreach (var row in itemIDs)
            {
                newWhere += $"'{row}',";
            }

            newWhere = newWhere.Substring(0, newWhere.Length - 1); //chop off last comma
            newWhere += ")";

            return newWhere;
        }
    }
}