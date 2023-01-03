using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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
                    cmd.CommandText = $"SELECT * FROM {AppConfig.DatabaseName}.signals WHERE SignalID <> -1 and include = 1";
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
            zoneGroups.Add("All");
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
                    cmd.CommandText =
                        "SELECT DISTINCT(Zone) FROM signals WHERE Zone IS NOT NULL and Zone <> '' and signalid <> -1 and include = 1 ORDER BY Zone ASC";
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
                        "SELECT DISTINCT(Zone) FROM signals WHERE Zone IS NOT NULL and signalid <> -1 and include = 1";
                    string where = "";
                    switch (zoneGroupName.Trim().ToUpper())
                    {
                        case "ALL RTOP":
                            where += " AND TRIM(UPPER(Zone_Group)) IN ('RTOP1','RTOP2')";
                            break;
                        case "Zone 7":
                            where += " AND TRIM(UPPER(Zone_Group)) IN ('ZONE 7M', 'ZONE 7D')";
                            break;
                        case "ALL":
                            where += "";
                            break;
                        default:
                            where += " AND TRIM(UPPER(Zone_Group)) = @zoneGroupName";
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
                        "SELECT DISTINCT(Corridor) FROM signals WHERE Corridor IS NOT NULL and corridor <> '' and signalid <> -1 and include = 1";
                    string where = "";
                    switch (zoneGroupName.Trim().ToUpper())
                    {
                        case "ALL RTOP":
                            where += " AND TRIM(UPPER(Zone_Group)) IN ('RTOP1','RTOP2')";
                            break;
                        case "Zone 7":
                            where += " AND TRIM(UPPER(Zone_Group)) IN ('ZONE 7M', 'ZONE 7D')";
                            break;
                        case "ALL":
                            break;
                        default:
                            where += " AND TRIM(UPPER(Zone_Group)) = @zoneGroupName";
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

        public static async Task<IEnumerable<string>> GetCountiesSQL(MySqlConnection sqlConnection)
        {
            var counties = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        "SELECT DISTINCT(County) FROM signals WHERE County IS NOT NULL and County <> '' and signalid <> -1 and include = 1 order by County ASC";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        counties.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetCountiesSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return counties;
        }

        public static async Task<IEnumerable<string>> GetCitiesSQL(MySqlConnection sqlConnection)
        {
            var cities = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText =
                        "SELECT DISTINCT(City) FROM signals WHERE City IS NOT NULL and City <> '' and signalid <> -1 and include = 1 order by City ASC";

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        cities.Add(reader.GetString(0).Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetCitiesSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return cities;
        }

        public static async Task<IEnumerable<string>> GetPrioritiesSQL(MySqlConnection sqlConnection)
        {
            var priorities = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using var cmd = new MySqlCommand();
                cmd.Connection = sqlConnection;
                cmd.CommandText =
                    "SELECT DISTINCT(Priority) FROM signals WHERE Priority IS NOT NULL and Priority <> '' and signalid <> -1 and include = 1 order by Priority ASC";

                await using var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    priorities.Add(reader.GetString(0).Trim());
                }
            }
            catch (Exception ex)
            {
                await WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetCitiesSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return priorities;
        }

        public static async Task<IEnumerable<string>> GetClassificationsSQL(MySqlConnection sqlConnection)
        {
            var classifications = new List<string>();
            try
            {
                await sqlConnection.OpenAsync();
                await using var cmd = new MySqlCommand();
                cmd.Connection = sqlConnection;
                cmd.CommandText =
                    "SELECT DISTINCT(Classification) FROM signals WHERE Classification IS NOT NULL and Classification <> '' and signalid <> -1 and include = 1 order by Classification ASC";

                await using var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    classifications.Add(reader.GetString(0).Trim());
                }
            }
            catch (Exception ex)
            {
                await WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetCitiesSQL", ex);
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

            return classifications;
        }

        public static async Task WriteToSignals(MySqlConnection sqlConnection, ExcelWorksheet ws)
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
            tbl.Columns.Add("TeamsGuid", typeof(string));
            tbl.Columns.Add("Priority", typeof(string));
            tbl.Columns.Add("Classification", typeof(string));

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
            await using var cmd = new MySqlCommand();
            cmd.Connection = sqlConnection;
            cmd.CommandText = $"TRUNCATE TABLE {AppConfig.DatabaseName}.signals";
            cmd.ExecuteNonQuery();

            var bulkCopy = new MySqlBulkCopy(sqlConnection);
            bulkCopy.DestinationTableName = $"{AppConfig.DatabaseName}.signals";
            await bulkCopy.WriteToServerAsync(tbl);
        }

        public static async Task<FilteredItems> GetCorridorsOrSignalsByFilter(MySqlConnection sqlConnection,
            FilterDTO filter)
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

        /// <summary>
        /// Returns a list of signals with their corresponding corridors.
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Signal>> GetSignalsWithCorridors(MySqlConnection sqlConnection, FilterDTO filter)
        {
            var signals = new List<Signal>();
            await using (var cmd = new MySqlCommand())
            {
                var where = CreateSignalsWhereClause(filter.zone_Group, filter.zone, filter.agency, filter.county,
                    filter.city, filter.corridor, filter.subcorridor, filter.signalId, filter.priority, filter.classification, cmd);
                var sqlText = $"SELECT SignalID, Corridor, Zone_Group FROM {AppConfig.DatabaseName}.signals WHERE Include = 1 {where}";
                cmd.Connection = sqlConnection;
                cmd.CommandText = sqlText;
                await sqlConnection.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    signals.Add(new Signal
                    {
                        SignalId = reader.GetString(0).Replace("'", "''").Trim(),
                        Corridor = reader.GetString(1).Replace("'", "''").Trim(),
                        ZoneGroup = reader.GetString(2).Trim()
                    });
                }
            }

            await sqlConnection.CloseAsync();

            return signals;
        }

        public static async Task<FilteredItems> GetSignalsByFilter(MySqlConnection sqlConnection, FilterDTO filter)
        {
            var filterType = GenericEnums.FilteredItemType.Signal;
            var retVal = new List<string>();
            await using (var cmd = new MySqlCommand())
            {
                var where = CreateSignalsWhereClause(filter.zone_Group, filter.zone, filter.agency, filter.county,
                    filter.city, filter.corridor, filter.subcorridor, filter.signalId, filter.priority, filter.classification, cmd);
                var sqlText = $"select distinct(signalid) from {AppConfig.DatabaseName}.signals where include = 1" + where;
                cmd.Connection = sqlConnection;
                cmd.CommandText = sqlText;
                await sqlConnection.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    retVal.Add(reader.GetString(0).Replace("'", "''").Trim());
                }
            }

            await sqlConnection.CloseAsync();
            return new FilteredItems { FilterType = filterType, Items = retVal };
        }

        public static async Task<FilteredItems> GetCorridorsByFilter(MySqlConnection sqlConnection, FilterDTO filter)
        {
            var retVal = new List<string>();
            var filterType = GenericEnums.FilteredItemType.Corridor;
            await using (var cmd = new MySqlCommand())
            {
                var where = CreateSignalsWhereClause(filter.zone_Group, filter.zone, filter.agency, filter.county,
                    filter.city, filter.corridor, filter.subcorridor, filter.signalId, filter.priority, filter.classification, cmd);
                var sqlText = $"select distinct(corridor) from {AppConfig.DatabaseName}.signals where include = 1" + where;
                if (filter.zone_Group == "All")
                {
                    sqlText = $"SELECT DISTINCT(Zone_Group) FROM {AppConfig.DatabaseName}.signals WHERE Zone_Group IS NOT NULL";
                }
                else
                {
                    sqlText = $"select distinct(corridor) from {AppConfig.DatabaseName}.signals where include = 1" + where;
                }

                await sqlConnection.OpenAsync();
                cmd.Connection = sqlConnection;
                cmd.CommandText = sqlText;
                await using var reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    retVal.Add(reader.GetString(0).Replace("'", "''").Trim());
                }
            }

            await sqlConnection.CloseAsync();
            return new FilteredItems { FilterType = filterType, Items = retVal };
        }

        private static string CreateSignalsWhereClause(string zoneGroup, string zone, string agency,
            string county, string city, string corridor, string subcorridor, string signalId, string priority, string classification, MySqlCommand cmd)
        {
            if (!signalId.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("SignalID", signalId);
                return " and SignalID = @SignalID";
            }

            if ((zoneGroup.IsStringNullOrBlank() || zoneGroup == "All") && zone.IsStringNullOrBlank() &&
                agency.IsStringNullOrBlank() &&
                county.IsStringNullOrBlank() && city.IsStringNullOrBlank() && corridor.IsStringNullOrBlank() &&
                subcorridor.IsStringNullOrBlank() && signalId.IsStringNullOrBlank() && priority.IsStringNullOrBlank() && classification.IsStringNullOrBlank())
            {
                return string.Empty;
            }

            var where = " and ";

            if (!zoneGroup.IsStringNullOrBlank() && zoneGroup != "All")
            {
                cmd.Parameters.AddWithValue("zoneGroup", zoneGroup);
                where += "Zone_Group = @zoneGroup and ";
            }

            if (!zone.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("zone", zone);
                where += "zone = @zone and ";
            }

            if (!agency.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("agency", agency);
                where += "agency = @agency and ";
            }

            if (!county.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("county", county);
                where += "county = @county and ";
            }

            if (!city.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("city", city);
                where += "city = @city and ";
            }

            if (!corridor.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("corridor", corridor);
                where += "corridor = @corridor and ";
            }

            if (!subcorridor.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("subcorridor", subcorridor);
                where += "subcorridor = @subcorridor and ";
            }

            if (!priority.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("priority", priority);
                where += "Priority = @priority and ";
            }

            if (!classification.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("classification", classification);
                where += "Classification = @classification and ";
            }

            //chop off the last 'and'
            where = where.Substring(0, where.Length - 4);

            return where;
        }
    }
}
