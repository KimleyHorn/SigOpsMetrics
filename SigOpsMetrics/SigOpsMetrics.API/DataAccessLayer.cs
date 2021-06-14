using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;
using MySqlConnector;
using OfficeOpenXml;
using SigOpsMetrics.API.Classes.DTOs;
#pragma warning disable 1591

namespace SigOpsMetrics.API
{
    public static class DataAccessLayer
    {
        private const string ApplicationName = "SigOpsMetrics.API";

        public static async Task<DataTable> GetMetric(MySqlConnection sqlConnection, string source, string level,
            string interval, string measure, DateTime start, DateTime end)
        {
            var whereClause = CreateDateRangeClause(interval, measure, start, end);
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
                await WriteToErrorLog(sqlConnection, System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    nameof(GetFromDatabase), ex);
                //Invalid configuration
                return new DataTable();
            }

        }

        private static string CreateDateRangeClause(string interval, string measure, DateTime start, DateTime end)
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
                    if (measure == "aogh") //todo more hourly measures?
                    {
                        period = "hour";
                        startFormat = start.ToString("yyyy-MM-dd");
                        endFormat = end.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        period = "month";
                        startFormat = start.ToString("yyyy-MM-dd");
                        endFormat = end.ToString("yyyy-MM-dd");
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
            return $" and Zone_Group = '{zoneGroup}'";
        }

        private static string CreateCorridorAndClause(string corridor)
        {
            return $" and Corridor = '{corridor}'";
        }

        private static string CreateDateRangeAndZoneGroupClause(string level, string measure, DateTime start, DateTime end, string zoneGroup)
        {
            var dateRangeClause = CreateDateRangeClause(level, measure, start, end);
            if (dateRangeClause == string.Empty)
                return string.Empty;

            return dateRangeClause + CreateZoneGroupAndClause(zoneGroup);
        }

        private static string CreateDateRangeAndCorridorClause(string level, string measure, DateTime start, DateTime end,
            string corridor)
        {
            var dateRangeClause = CreateDateRangeClause(level, measure, start, end);
            if (dateRangeClause == string.Empty)
                return string.Empty;

            return dateRangeClause + CreateCorridorAndClause(corridor);
        }

        public static async Task WriteToErrorLog(MySqlConnection sqlConnection, string applicationName, string functionName, Exception ex)
        {
            await WriteToErrorLog(sqlConnection, applicationName, functionName, ex.Message, ex.InnerException?.ToString());
        }

        public static async Task WriteToErrorLog(MySqlConnection sqlConnection, string applicationName,
            string functionName, string exception, string innerException)
        {
            try
            {
                await sqlConnection.OpenAsync();
                using (var cmd = new MySqlCommand())
                {

                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        $"insert into mark1.errorlog (applicationname, functionname, exception, innerexception) values ('{applicationName}', '{functionName}', '{exception.Substring(0, exception.Length > 500 ? 500 : exception.Length)}', '{innerException}') ";
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static async Task<IEnumerable<SignalDTO>> GetAllSignalDataSQL(MySqlConnection sqlConnection)
        {
            List<SignalDTO> signals = new List<SignalDTO>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {

                    cmd.Connection = sqlConnection;
                    cmd.CommandText = "SELECT * FROM mark1.signals WHERE SignalID <> -1";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        SignalDTO row = new SignalDTO
                        {
                            SignalID = reader.IsDBNull(0) ? "" : reader.GetString(0).Trim(),
                            ZoneGroup = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim(),
                            Zone = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim(),
                            Corridor = reader.IsDBNull(3) ? "" : reader.GetString(3).Trim(),
                            Subcorridor = reader.IsDBNull(4) ? "" : reader.GetString(4).Trim(),
                            Agency = reader.IsDBNull(5) ? "" : reader.GetString(5).Trim(),
                            MainStreetName = reader.IsDBNull(6) ? "" : reader.GetString(6).Trim(),
                            SideStreetName = reader.IsDBNull(7) ? "" : reader.GetString(7).Trim(),
                            Milepost = reader.IsDBNull(8) ? "" : reader.GetString(8).Trim(),
                            AsOf = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                            Duplicate = reader.IsDBNull(10) ? "" : reader.GetString(10).Trim(),
                            Include = reader.IsDBNull(11) ? "" : reader.GetString(11).Trim(),
                            Modified = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12),
                            Note = reader.IsDBNull(13) ? "" : reader.GetString(13).Trim(),
                            Latitude = reader.IsDBNull(14) ? 0 : reader.GetDouble(14),
                            Longitude = reader.IsDBNull(15) ? 0 : reader.GetDouble(15),
                            County = reader.IsDBNull(16) ? "" : reader.GetString(16).Trim(),
                            City = reader.IsDBNull(17) ? "" : reader.GetString(17).Trim()
                        };
                        if (row.AsOf == DateTime.Parse("1899-12-31T00:00:00"))
                            row.AsOf = null;
                        if (row.Modified == DateTime.Parse("1899-12-31T00:00:00"))
                            row.Modified = null;
                        signals.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                await WriteToErrorLog(sqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetAllSignalDataSQL", ex);
            }
            finally
            {
                sqlConnection.Close();
            }
            return signals;
        }

        public static async Task<IEnumerable<SignalDTO>> GetSignalMetricDataSQL(MySqlConnection sqlConnection, string source, string level, string interval, string measure, DateTime start, DateTime end, string metric)
        {
            List<SignalDTO> signals = new List<SignalDTO>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    var whereClause = CreateDateRangeClause(interval, measure, start, end);
                    cmd.CommandText = $"SELECT S.* " +
                        $", M.{metric} " +
                        $"FROM mark1.signals S " +
                        $"LEFT JOIN (SELECT Corridor, {metric} FROM mark1.{level}_{interval}_{measure} {whereClause}) M ON S.SignalID = M.Corridor OR S.Corridor = M.Corridor " +
                        $"WHERE S.SignalID <> -1 AND S.Include = 1";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        SignalDTO row = new SignalDTO
                        {
                            SignalID = reader.IsDBNull(0) ? "" : reader.GetString(0).Trim(),
                            ZoneGroup = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim(),
                            Zone = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim(),
                            Corridor = reader.IsDBNull(3) ? "" : reader.GetString(3).Trim(),
                            Subcorridor = reader.IsDBNull(4) ? "" : reader.GetString(4).Trim(),
                            Agency = reader.IsDBNull(5) ? "" : reader.GetString(5).Trim(),
                            MainStreetName = reader.IsDBNull(6) ? "" : reader.GetString(6).Trim(),
                            SideStreetName = reader.IsDBNull(7) ? "" : reader.GetString(7).Trim(),
                            Milepost = reader.IsDBNull(8) ? "" : reader.GetString(8).Trim(),
                            AsOf = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9),
                            Duplicate = reader.IsDBNull(10) ? "" : reader.GetString(10).Trim(),
                            Include = reader.IsDBNull(11) ? "" : reader.GetString(11).Trim(),
                            Modified = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12),
                            Note = reader.IsDBNull(13) ? "" : reader.GetString(13).Trim(),
                            Latitude = reader.IsDBNull(14) ? 0 : reader.GetDouble(14),
                            Longitude = reader.IsDBNull(15) ? 0 : reader.GetDouble(15),
                            County = reader.IsDBNull(16) ? "" : reader.GetString(16).Trim(),
                            City = reader.IsDBNull(17) ? "" : reader.GetString(17).Trim()
                        };
                        if (row.AsOf == DateTime.Parse("1899-12-31T00:00:00"))
                            row.AsOf = null;
                        if (row.Modified == DateTime.Parse("1899-12-31T00:00:00"))
                            row.Modified = null;
                        signals.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                await WriteToErrorLog(sqlConnection,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                "GetAllSignalDataSQL", ex);
            }
            finally
            {
                sqlConnection.Close();
            }
            return signals;
        }

        public static async Task WriteToSignals(MySqlConnection sqlConnection, ExcelWorksheet ws)
        {
            try
            {
                DataTable tbl = new DataTable();
                tbl.Columns.Add("SignalID", typeof(string));
                tbl.Columns.Add("Zone_Group", typeof(string));
                tbl.Columns.Add("Zone", typeof(string));
                tbl.Columns.Add("Corridor", typeof(string));
                tbl.Columns.Add("Subcorridor", typeof(string));
                tbl.Columns.Add("Agency", typeof(string));
                tbl.Columns.Add("Main_Street_Name", typeof(string));
                tbl.Columns.Add("Side_Street_Name", typeof(string));
                tbl.Columns.Add("Milepost", typeof(string));
                tbl.Columns.Add("Asof", typeof(DateTime));
                tbl.Columns.Add("Duplicate", typeof(string));
                tbl.Columns.Add("Include", typeof(string));
                tbl.Columns.Add("Modified", typeof(DateTime));
                tbl.Columns.Add("Note", typeof(string));
                tbl.Columns.Add("Latitude", typeof(decimal));
                tbl.Columns.Add("Longitude", typeof(decimal));
                tbl.Columns.Add("County", typeof(string));
                tbl.Columns.Add("City", typeof(string));

                var startRow = 2;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    DataRow row = tbl.Rows.Add();
                    foreach (var cell in wsRow)
                    {
                        if (!string.IsNullOrEmpty(cell.Text))
                        {
                            if (cell.Text == "#REF!")
                            {
                                continue;
                            }
                            switch (cell.Start.Column)
                            {
                                case 10:
                                case 13:
                                    row[cell.Start.Column - 1] = DateTime.Parse(cell.Text).ToString("MM/dd/yyyy");
                                    break;
                                default:
                                    row[cell.Start.Column - 1] = cell.Text;
                                    break;
                            }
                        }
                    }
                }

                sqlConnection.ConnectionString += ";AllowLoadLocalInfile=True";
                await sqlConnection.OpenAsync();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText = "TRUNCATE TABLE mark1.signals";
                    cmd.ExecuteNonQuery();

                    var bulkCopy = new MySqlBulkCopy(sqlConnection);
                    bulkCopy.DestinationTableName = "mark1.signals";
                    bulkCopy.WriteToServer(tbl);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
