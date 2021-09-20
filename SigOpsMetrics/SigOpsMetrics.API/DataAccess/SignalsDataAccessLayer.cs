using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using OfficeOpenXml;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.DTOs;
using SigOpsMetrics.API.Classes.Extensions;
using SigOpsMetrics.API.Classes.Internal;

#pragma warning disable 1591

namespace SigOpsMetrics.API.DataAccess
{
    public class SignalsDataAccessLayer : BaseDataAccessLayer
    {
        public static async Task<IEnumerable<SignalDTO>> GetAllSignalDataSQL(MySqlConnection sqlConnection)
        {
            var signals = new List<SignalDTO>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {

                    cmd.Connection = sqlConnection;
                    cmd.CommandText = "SELECT * FROM mark1.signals WHERE SignalID <> -1 and include = 1";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        var row = new SignalDTO
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
                            AsOf = reader.IsDBNull(9) ? (DateTime?) null : reader.GetDateTime(9),
                            Duplicate = reader.IsDBNull(10) ? "" : reader.GetString(10).Trim(),
                            Include = reader.IsDBNull(11) ? "" : reader.GetString(11).Trim(),
                            Modified = reader.IsDBNull(12) ? (DateTime?) null : reader.GetDateTime(12),
                            Note = reader.IsDBNull(13) ? "" : reader.GetString(13).Trim(),
                            Latitude = reader.IsDBNull(14) ? 0 : reader.GetDouble(14),
                            Longitude = reader.IsDBNull(15) ? 0 : reader.GetDouble(15),
                            County = reader.IsDBNull(16) ? "" : reader.GetString(16).Trim(),
                            City = reader.IsDBNull(17) ? "" : reader.GetString(17).Trim()
                        };
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
                await sqlConnection.CloseAsync();
            }

            return signals;
        }

        public static async Task<IEnumerable<string>> GetSignalNamesSQL(MySqlConnection sqlConnection)
        {
            var signals = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {

                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        "SELECT CONCAT(TRIM(Main_Street_Name),' @ ', TRIM(Side_Street_Name)) FROM signals WHERE Main_Street_Name IS NOT NULL AND Side_Street_Name IS NOT NULL and signalid <> -1 and include = 1";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        signals.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetSignalNamesSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return signals;
        }

        public static async Task<IEnumerable<string>> GetZoneGroupsSQL(MySqlConnection sqlConnection)
        {
            var zoneGroups = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {

                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        "SELECT DISTINCT(Zone_Group) FROM signals WHERE Zone_Group IS NOT NULL and signalid <> -1 and include = 1 ORDER BY Zone_Group ASC";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        zoneGroups.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetZoneGroupsSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return zoneGroups;
        }

        public static async Task<IEnumerable<string>> GetZonesSQL(MySqlConnection sqlConnection)
        {
            var zones = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {

                    cmd.Connection = sqlConnection;
                    cmd.CommandText = "SELECT DISTINCT(Zone) FROM signals WHERE Zone IS NOT NULL and Zone <> '' and signalid <> -1 and include = 1 ORDER BY Zone ASC";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        zones.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetZonesSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return zones;
        }


        public static async Task<IEnumerable<string>> GetZonesByZoneGroupSQL(MySqlConnection sqlConnection,
            string zoneGroupName)
        {
            var zones = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        "SELECT DISTINCT(Zone) FROM signals WHERE Zone IS NOT NULL and Zone <> '' and signalid <> -1 and include = 1 AND TRIM(UPPER(Zone_Group))";
                    var where = "";
                    switch (zoneGroupName.Trim().ToUpper())
                    {
                        case "ALL RTOP":
                            where += " IN ('RTOP1','RTOP2')";
                            break;
                        case "Zone 7":
                            where += " IN ('ZONE 7M', 'ZONE 7D')";
                            break;
                        default:
                            where += " = @zoneGroupName";
                            cmd.Parameters.AddWithValue("zoneGroupName", zoneGroupName.Trim().ToUpper());
                            break;
                    }

                    cmd.CommandText += where;

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        zones.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetZonesByZoneGroupSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return zones;
        }

        public static async Task<IEnumerable<string>> GetCorridorsByZoneGroupSQL(MySqlConnection sqlConnection,
            string zoneGroupName)
        {
            var zones = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        "SELECT DISTINCT(Corridor) FROM signals WHERE Corridor IS NOT NULL and corridor <> '' and signalid <> -1 and include = 1 AND TRIM(UPPER(Zone_Group))";
                    var where = "";
                    switch (zoneGroupName.Trim().ToUpper())
                    {
                        case "ALL RTOP":
                            where += " IN ('RTOP1','RTOP2')";
                            break;
                        case "Zone 7":
                            where += " IN ('ZONE 7M', 'ZONE 7D')";
                            break;
                        default:
                            where += " = @zoneGroupName";
                            cmd.Parameters.AddWithValue("zoneGroupName", zoneGroupName.Trim().ToUpper());
                            break;
                    }

                    cmd.CommandText += where;

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        zones.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetCorridorsByZoneGroupSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return zones;
        }

        public static async Task<IEnumerable<string>> GetCorridorsSQL(MySqlConnection sqlConnection)
        {
            var corridors = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        "SELECT DISTINCT(Corridor) FROM signals WHERE Corridor IS NOT NULL and corridor <> '' and signalid <> -1 and include = 1 order by Corridor ASC";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        corridors.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetCorridorsSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return corridors;
        }

        public static async Task<IEnumerable<string>> GetCorridorsByZoneSQL(MySqlConnection sqlConnection,
            string zoneName)
        {
            var corridors = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        "SELECT DISTINCT(Corridor) FROM signals WHERE TRIM(Zone) = @zoneName and Corridor is not null and corridor <> '' and signalid <> -1 and include = 1 order by Corridor ASC";
                    cmd.Parameters.AddWithValue("zoneName", zoneName.Trim());

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        corridors.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetCorridorsByZoneSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return corridors;
        }

        public static async Task<IEnumerable<string>> GetSubCorridorsSQL(MySqlConnection sqlConnection)
        {
            var subcorridors = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        "SELECT DISTINCT(Subcorridor) FROM signals WHERE Subcorridor IS NOT NULL and Corridor is not null and subcorridor <> '' and signalid <> -1 and include = 1 order by Subcorridor ASC";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        subcorridors.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetSubCorridorsSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return subcorridors;
        }

        public static async Task<IEnumerable<string>> GetSubCorridorsByCorridorSQL(MySqlConnection sqlConnection,
            string corridor)
        {
            var subCorridors = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        "SELECT DISTINCT(SubCorridor) FROM signals WHERE TRIM(Corridor) = @corridor AND Subcorridor IS NOT NULL and signalid <> -1 and include = 1 order by Subcorridor ASC";
                    cmd.Parameters.AddWithValue("corridor", corridor.Trim());

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        subCorridors.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetSubCorridorsByCorridorSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return subCorridors;
        }

        public static async Task<IEnumerable<string>> GetAgenciesSQL(MySqlConnection sqlConnection)
        {
            var agencies = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        "SELECT DISTINCT(Agency) FROM signals WHERE Agency IS NOT NULL and Agency <> '' and signalid <> -1 and include = 1 order by Agency ASC";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        agencies.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetAgenciesSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return agencies;
        }

        public static async Task WriteToSignals(MySqlConnection sqlConnection, ExcelWorksheet ws)
        {
            try
            {
                var tbl = new DataTable();
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
                for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    var row = tbl.Rows.Add();
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
                                case 12: //Include
                                    row[cell.Start.Column - 1] =
                                        cell.Text.ToUpper() == "TRUE" || cell.Text == "1" ? 1 : 0;
                                    break;
                                case 10: //Asof
                                case 13: //Modified
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

        public static async Task<FilteredItems> GetCorridorsOrSignalsByFilter(MySqlConnection sqlConnection, FilterDTO filter)
        {
            //Based on the filter, we want to return a list of either corridors or signals
            if (filter.corridor.IsStringNullOrBlank())
            {
                return await GetCorridorsByFilter(sqlConnection, filter);
            }
            else
            {
                return await GetSignalsByFilter(sqlConnection, filter);
            }

        }

        public static async Task<FilteredItems> GetSignalsByFilter(MySqlConnection sqlConnection, FilterDTO filter)
        {
            var filterType = GenericEnums.FilteredItemType.Signal;
            var where = CreateSignalsWhereClause(filter.zone_Group, filter.zone, filter.agency, filter.county,
                filter.city, filter.corridor, filter.subcorridor);
            var sqlText = "select distinct(signalid) from mark1.signals " + where + " and include = 1";

            var retVal = new List<string>();
            await sqlConnection.OpenAsync();
            await using (var cmd = new MySqlCommand())
            {
                cmd.Connection = sqlConnection;
                cmd.CommandText = sqlText;
                await using var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    retVal.Add(reader.GetString(0).Trim());
                }
            }

            await sqlConnection.CloseAsync();
            return new FilteredItems { FilterType = filterType, Items = retVal };
        }

        public static async Task<FilteredItems> GetCorridorsByFilter(MySqlConnection sqlConnection, FilterDTO filter)
        {
            var filterType = GenericEnums.FilteredItemType.Corridor;
            var where = CreateSignalsWhereClause(filter.zone_Group, filter.zone, filter.agency, filter.county,
                filter.city, filter.corridor, filter.subcorridor);
            var sqlText = "select distinct(corridor) from mark1.signals " + where + " and include = 1";

            var retVal = new List<string>();
            await sqlConnection.OpenAsync();
            await using (var cmd = new MySqlCommand())
            {
                cmd.Connection = sqlConnection;
                cmd.CommandText = sqlText;
                await using var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    retVal.Add(reader.GetString(0).Trim());
                }
            }

            await sqlConnection.CloseAsync();
            return new FilteredItems { FilterType = filterType, Items = retVal };
        }

        private static string CreateSignalsWhereClause(string zoneGroup, string zone, string agency,
            string county, string city, string corridor, string subcorridor)
        {
            if (zoneGroup.IsStringNullOrBlank() && zone.IsStringNullOrBlank() && agency.IsStringNullOrBlank() &&
                county.IsStringNullOrBlank() && city.IsStringNullOrBlank() && corridor.IsStringNullOrBlank() && subcorridor.IsStringNullOrBlank())
                return string.Empty;
            var where = "where ";

            if (!zoneGroup.IsStringNullOrBlank())
                where += $"Zone_Group = '{zoneGroup}' and ";
            if (!zone.IsStringNullOrBlank())
                where += $"zone = '{zone}' and ";
            if (!agency.IsStringNullOrBlank())
                where += $"agency = '{agency}' and ";
            if (!county.IsStringNullOrBlank())
                where += $"county = '{county}' and ";
            if (!city.IsStringNullOrBlank())
                where += $"city = '{city}' and ";
            if (!corridor.IsStringNullOrBlank())
                where += $"corridor = '{corridor}' and ";
            if (!subcorridor.IsStringNullOrBlank())
                where += $"subcorridor = '{subcorridor}' and ";

            //chop off the last 'and'
            where = where.Substring(0, where.Length - 4);

            return where;


        }


    }
}
