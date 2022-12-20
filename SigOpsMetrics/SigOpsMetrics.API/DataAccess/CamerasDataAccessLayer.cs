using MySqlConnector;
using OfficeOpenXml;
using SigOpsMetrics.API.Classes;
using SigOpsMetrics.API.Classes.DTOs;
using SigOpsMetrics.API.Classes.Extensions;
using SigOpsMetrics.API.Classes.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SigOpsMetrics.API.DataAccess
{
    /// <summary>
    /// Data access for CCTV
    /// </summary>
    public class CamerasDataAccessLayer : BaseDataAccessLayer
    {
        /// <summary>
        /// Save worksheet to cameras table
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="ws"></param>
        public static async Task WriteToCameras(MySqlConnection sqlConnection, ExcelWorksheet ws)
        {
            try
            {
                var tbl = new DataTable();
                tbl.Columns.Add("CameraID", typeof(string));
                tbl.Columns.Add("Location", typeof(string));
                tbl.Columns.Add("Zone_Group", typeof(string));
                tbl.Columns.Add("Zone", typeof(string));
                tbl.Columns.Add("Corridor", typeof(string));
                tbl.Columns.Add("As_of_Date", typeof(DateTime));
                tbl.Columns.Add("Roadway_Name", typeof(string));
                tbl.Columns.Add("Cross_Street", typeof(string));
                tbl.Columns.Add("IP", typeof(string));
                tbl.Columns.Add("Manufacturer", typeof(string));
                tbl.Columns.Add("MaxviewID", typeof(string));
                tbl.Columns.Add("RTOP_Comm_Method", typeof(string));
                tbl.Columns.Add("Include", typeof(string));
                tbl.Columns.Add("Initial_Search_Set", typeof(string));
                tbl.Columns.Add("Pre_existing_ID", typeof(string));
                tbl.Columns.Add("Status", typeof(string));
                tbl.Columns.Add("UnconfirmedMvID", typeof(string));
                tbl.Columns.Add("Column1", typeof(string));
                tbl.Columns.Add("511Endpoint", typeof(string));
                tbl.Columns.Add("Managing_Agency", typeof(string));
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
                                case 13: //Include
                                    row[cell.Start.Column - 1] =
                                        cell.Text.ToUpper() == "TRUE" || cell.Text == "1" ? 1 : 0;
                                    break;

                                case 6: //As_Of_Date
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
                cmd.CommandText = $"TRUNCATE TABLE {AppConfig.DatabaseName}.cameras";
                cmd.ExecuteNonQuery();

                var bulkCopy = new MySqlBulkCopy(sqlConnection)
                {
                    DestinationTableName = $"{AppConfig.DatabaseName}.cameras"
                };
                await bulkCopy.WriteToServerAsync(tbl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Get cameras from database for given filter
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Cctv>> GetCameras(MySqlConnection sqlConnection,
            FilterDTO filter)
        {
            try
            {
                var cctvs = new List<Cctv>();
                await using (var cmd = new MySqlCommand())
                {
                    var where = CreateWhereClause(filter.zone_Group, filter.zone, filter.agency, filter.county,
                        filter.city, filter.corridor, filter.signalId, cmd);
                    var sqlText = $"SELECT CameraID, Zone_Group, Corridor FROM {AppConfig.DatabaseName}.cameras WHERE Include = 1 {where}";
                    cmd.Connection = sqlConnection;
                    cmd.CommandText = sqlText;
                    await sqlConnection.OpenAsync();
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        var cctv = new Cctv
                        {
                            CameraId = reader.GetString(0).Replace("'", "''").Trim(),
                            ZoneGroup = reader.GetValue(1) as string,
                            Corridor = reader.GetValue(2) as string
                        };
                        cctv.ZoneGroup = cctv.ZoneGroup?.Replace("'", "''").Trim();
                        cctv.Corridor = cctv.Corridor?.Replace("'", "''").Trim();
                        cctvs.Add(cctv);
                    }
                }

                await sqlConnection.CloseAsync();

                return cctvs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private static string CreateWhereClause(string zoneGroup, string zone, string agency,
            string county, string city, string corridor, string cameraId, MySqlCommand cmd)
        {
            if (!cameraId.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("CameraID", cameraId);
                return " and CameraID = @CameraID";
            }

            if ((zoneGroup.IsStringNullOrBlank() || zoneGroup == "All") && zone.IsStringNullOrBlank() &&
                agency.IsStringNullOrBlank() &&
                county.IsStringNullOrBlank() && city.IsStringNullOrBlank() && corridor.IsStringNullOrBlank() &&
                cameraId.IsStringNullOrBlank())
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
                where += "Zone = @zone and ";
            }

            if (!agency.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("agency", agency);
                where += "Managing_Agency = @agency and ";
            }

            if (!county.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("county", county);
                where += "County = @county and ";
            }

            if (!city.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("city", city);
                where += "City = @city and ";
            }

            if (!corridor.IsStringNullOrBlank())
            {
                cmd.Parameters.AddWithValue("corridor", corridor);
                where += "Corridor = @corridor and ";
            }

            //chop off the last 'and'
            where = where.Substring(0, where.Length - 4);

            return where;
        }
    }
}