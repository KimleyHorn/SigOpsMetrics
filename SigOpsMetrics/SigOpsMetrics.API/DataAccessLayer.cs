﻿using System;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
#pragma warning disable 1591

namespace SigOpsMetrics.API
{
    public static class DataAccessLayer
    {
        public static async Task<DataTable> GetMetric(MySqlConnection sqlConnection, string source, string level,
            string interval, string measure, DateTime start, DateTime end)
        {
            var whereClause = CreateDateRangeClause(interval, start, end);
            if (whereClause == string.Empty)
                return new DataTable();

            return await GetFromDatabase(sqlConnection, level, interval, measure, whereClause);
        }

        public static async Task<DataTable> GetMetricByZoneGroup(MySqlConnection sqlConnection, string source, string level,
            string interval, string measure, DateTime start, DateTime end, string zoneGroup)
        {
            var whereClause = CreateDateRangeAndZoneGroupClause(interval, start, end, zoneGroup);
            if (whereClause == string.Empty)
                return new DataTable();

            return await GetFromDatabase(sqlConnection, level, interval, measure, whereClause);
        }

        public static async Task<DataTable> GetMetricByCorridor(MySqlConnection sqlConnection, string source, string level,
            string interval, string measure, DateTime start, DateTime end, string corridor)
        {
            var whereClause = CreateDateRangeAndCorridorClause(interval, start, end, corridor);
            if (whereClause == string.Empty)
                return new DataTable();

            return await GetFromDatabase(sqlConnection, level, interval, measure, whereClause);
        }

        private static async Task<DataTable> GetFromDatabase(MySqlConnection sqlConnection, string level, string interval, string measure,
            string whereClause)
        {
            try
            {
                await sqlConnection.OpenAsync();
                await using var command =
                    new MySqlCommand($"select * from mark1.{level}_{interval}_{measure} {whereClause}", sqlConnection);
                await using var reader = await command.ExecuteReaderAsync();

                var tb = new DataTable();
                tb.Load(reader);
                return tb;
            }
            catch (Exception ex)
            {
                //Invalid configuration
                return new DataTable();
            }

        }

        private static string CreateDateRangeClause(string interval, DateTime start, DateTime end)
        {
            string period;
            string startFormat = start.ToString();
            string endFormat = end.ToString();

            switch (interval)
            {
                case "dy":
                case "wk":
                    period = "date";
                    break;
                case "mo":
                    period = "month";
                    startFormat = start.ToString("yyyy-MM-dd");
                    endFormat = end.ToString("yyyy-MM-dd");
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
            return $" and Zone_Group = '{zoneGroup}'";
        }

        private static string CreateCorridorAndClause(string corridor)
        {
            return $" and Corridor = '{corridor}'";
        }

        private static string CreateDateRangeAndZoneGroupClause(string level, DateTime start, DateTime end, string zoneGroup)
        {
            var dateRangeClause = CreateDateRangeClause(level, start, end);
            if (dateRangeClause == string.Empty)
                return string.Empty;

            return dateRangeClause + CreateZoneGroupAndClause(zoneGroup);
        }

        private static string CreateDateRangeAndCorridorClause(string level, DateTime start, DateTime end,
            string corridor)
        {
            var dateRangeClause = CreateDateRangeClause(level, start, end);
            if (dateRangeClause == string.Empty)
                return string.Empty;

            return dateRangeClause + CreateCorridorAndClause(corridor);
        }
    }
}
