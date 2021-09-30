using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MySqlConnector;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.Extensions;
using SigOpsMetrics.API.Classes.Internal;

#pragma warning disable 1591

namespace SigOpsMetrics.API.DataAccess
{
    public class MetricsDataAccessLayer : BaseDataAccessLayer
    {
        private const string ApplicationName = "SigOpsMetrics.API";

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
            var whereClause = CreateDateRangeAndZoneGroupClause(interval, measure, start, end, zoneGroup);
            if (whereClause == string.Empty)
                return new DataTable();

            return await GetFromDatabase(sqlConnection, level, interval, measure, whereClause);
        }

        public static async Task<DataTable> GetMetricByCorridor(MySqlConnection sqlConnection, string source, string level,
            string interval, string measure, DateTime start, DateTime end, string corridor)
        {
            var whereClause = CreateDateRangeAndCorridorClause(interval, measure, start, end, corridor);
            if (whereClause == string.Empty)
                return new DataTable();

            return await GetFromDatabase(sqlConnection, level, interval, measure, whereClause);
        }

        public static async Task<DataTable> GetMetricByFilter(MySqlConnection sqlConnection, string source, string measure,
            string interval, string start, string end, FilteredItems filteredItems, bool all = false)
        {
            var dateRangeClause = CreateDateRangeClause(interval, measure, start, end);
            var fullWhereClause = AddSignalsToWhereClause(dateRangeClause, filteredItems.Items);

            return await GetFromDatabase(sqlConnection,
                filteredItems.FilterType == GenericEnums.FilteredItemType.Corridor ? "cor" : "sig", interval, measure,
                fullWhereClause, all);
        }

        private static async Task<DataTable> GetFromDatabase(MySqlConnection sqlConnection, string level, string interval, string measure,
            string whereClause, bool all = false)
        {
            var tb = new DataTable();
            try
            {
                if (all)
                {
                    string type = level == "sig" ? "SignalId" : "Corridor";
                    await sqlConnection.OpenAsync();
                    await using var command =
                        new MySqlCommand($"select t.*, CASE WHEN s.Zone_Group IS NULL THEN t.Corridor ELSE s.Zone_Group END AS ActualZoneGroup from mark1.{level}_{interval}_{measure} t left join (select {type} AS SignalType, Zone_Group FROM mark1.signals GROUP BY {type}, Zone_Group) s ON s.SignalType = t.Corridor {whereClause}", sqlConnection);
                    await using var reader = await command.ExecuteReaderAsync();
                    tb.Load(reader);
                } else {
                    await sqlConnection.OpenAsync();
                    await using var command =
                        new MySqlCommand($"select * from mark1.{level}_{interval}_{measure} {whereClause}", sqlConnection);
                    await using var reader = await command.ExecuteReaderAsync();
                    tb.Load(reader);
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

        private static string CreateZoneGroupAndClause(string zoneGroup)
        {
            return zoneGroup == "All" ? "" : $" and Zone_Group = '{zoneGroup}'";
        }

        private static string CreateCorridorAndClause(string corridor)
        {
            return $" and Corridor = '{corridor}'";
        }

        private static string CreateDateRangeAndZoneGroupClause(string level, string measure, DateTime start, DateTime end, string zoneGroup)
        {
            var dateRangeClause = CreateDateRangeClause(level, measure, start.ToString(), end.ToString());
            if (dateRangeClause == string.Empty)
                return string.Empty;

            return dateRangeClause + CreateZoneGroupAndClause(zoneGroup);
        }

        private static string CreateDateRangeAndCorridorClause(string level, string measure, DateTime start, DateTime end,
            string corridor)
        {
            var dateRangeClause = CreateDateRangeClause(level, measure, start.ToString(), end.ToString());
            if (dateRangeClause == string.Empty)
                return string.Empty;

            return dateRangeClause + CreateCorridorAndClause(corridor);
        }

        private static string AddSignalsToWhereClause(string whereClause, List<string> itemIDs)
        {
            var newWhere = string.IsNullOrEmpty(whereClause) ? " where " : whereClause;

            newWhere += string.IsNullOrEmpty(whereClause) ? " Corridor in (" : " and Corridor in (";
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
