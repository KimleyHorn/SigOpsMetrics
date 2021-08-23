using MySqlConnector;
using SigOpsMetrics.API.Classes.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SigOpsMetrics.API.DataAccess
{
    public class WatchdogDataAccessLayer : BaseDataAccessLayer
    {
        public static async Task<List<WatchdogHeatmapDTO>> GetWatchdogData(MySqlConnection sqlConnection, WatchdogFilterRequestObject data)
        {
            List<WatchdogDTO> dataList = new List<WatchdogDTO>();
            List<WatchdogDTO> tableDataList = new List<WatchdogDTO>();
            try
            {
                await sqlConnection.OpenAsync();
                await using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = sqlConnection;
                    cmd.CommandText = @"SELECT Zone_Group AS ZoneGroup, Zone, Corridor, SignalID, Date, Name, Alert, streak AS Streak FROM mark1.WatchdogAlerts
                                        WHERE str_to_date(Date,'%Y-%m-%d') >= @startDate AND str_to_date(Date,'%Y-%m-%d') <= @endDate AND Alert = @alert";

                    cmd.Parameters.AddWithValue("startDate", data.StartDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("endDate", data.EndDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("alert", data.Alert);

                    if (!string.IsNullOrEmpty(data.Phase) && data.Phase != "All")
                    {
                        cmd.CommandText += " AND CallPhase = @phase";
                        cmd.Parameters.AddWithValue("phase", data.Phase);
                    }

                    if (!string.IsNullOrEmpty(data.IntersectionFilter))
                    {
                        cmd.CommandText += " AND (Name LIKE @intersectionFilter OR SignalId LIKE @intersectionFilter)";
                        cmd.Parameters.AddWithValue("intersectionFilter", "%" + data.IntersectionFilter + "%");
                    }

                    cmd.CommandText += " ORDER BY SignalID * 1 ASC, SignalID ASC";
                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        WatchdogDTO dto = new WatchdogDTO
                        {
                            ZoneGroup = reader.IsDBNull(0) ? "" : reader["ZoneGroup"].ToString().Trim(),
                            Zone = reader.IsDBNull(1) ? "" : reader["Zone"].ToString().Trim(),
                            Corridor = reader.IsDBNull(2) ? "" : reader["Corridor"].ToString().Trim(),
                            SignalID = reader.IsDBNull(3) ? "" : reader["SignalID"].ToString().Trim(),
                            Date = reader.IsDBNull(4) ? "" : reader["Date"].ToString().Trim(),
                            Name = reader.IsDBNull(5) ? "" : reader["Name"].ToString().Trim(),
                            Alert = reader.IsDBNull(6) ? "" : reader["Alert"].ToString().Trim(),
                            Streak = reader.IsDBNull(7) ? 0 : Int32.Parse(reader["Streak"].ToString().Trim())
                        };
                        dataList.Add(dto);
                    }
                }
            }
            catch (Exception ex)
            {
                await MetricsDataAccessLayer.WriteToErrorLog(sqlConnection,
                    System.Reflection.Assembly.GetEntryAssembly().GetName().Name,
                    "GetWatchdogData", ex);
            }
            finally
            {
                sqlConnection.Close();
            }

            WatchdogHeatmapDTO heatmap = new WatchdogHeatmapDTO
            {
                X = new List<string>(),
                Y = new List<string>(),
                Z = new List<List<int?>>()
            };

            int days = (int)(Math.Ceiling((data.EndDate - data.StartDate).TotalDays));
            for (int i=0;i<=days;i++)
            {
                heatmap.X.Add(data.StartDate.Date.AddDays(i).ToString("yyyy-MM-dd"));
            }

            var uniqueCombos = dataList.Select(x => new { x.Name, x.SignalID }).Distinct();
            foreach (var td in uniqueCombos)
            {
                if (!string.IsNullOrEmpty(data.Streak) && data.Streak != "All")
                {
                    if (data.Streak == "Active")
                    {
                        //Only get records where the last day is part of the streak?
                        if (dataList.Any(x => x.SignalID == td.SignalID && x.Name == td.Name && x.Date == heatmap.X[heatmap.X.Count - 1]))
                        {
                            heatmap.Y.Add(td.SignalID + ": " + td.Name);
                        }
                    }
                    else if (data.Streak == "Active 3-days" && heatmap.X.Count() >=3)
                    {
                        //Only get records where the last 3 days are part of the streak?
                        DateTime startDay = DateTime.Parse(heatmap.X[heatmap.X.Count - 3]);
                        DateTime endDate = DateTime.Parse(heatmap.X[heatmap.X.Count - 1]);
                        int dayCount = dataList.Where(x => x.SignalID == td.SignalID && x.Name == td.Name && DateTime.Parse(x.Date) <= endDate && DateTime.Parse(x.Date) >= startDay).Count();
                        if (dayCount == 3)
                        {
                            heatmap.Y.Add(td.SignalID + ": " + td.Name);
                        }
                    }
                } else
                {
                    heatmap.Y.Add(td.SignalID + ": " + td.Name);
                }
            }
            
            List<WatchdogDTO> sortedList = dataList.OrderByDescending(x => x.Streak).ToList();
            foreach (string s in heatmap.Y)
            {
                List<int?> streaks = new List<int?>();
                foreach (string date in heatmap.X)
                {
                    if (dataList.Any(x => x.SignalID + ": " + x.Name == s && x.Date == date))
                    {
                        int? streak = dataList.First(x => x.SignalID + ": " + x.Name == s && x.Date == date).Streak;
                        streaks.Add(streak);
                    } else
                    {
                        streaks.Add(0);
                    }
                }
                heatmap.Z.Add(streaks);
                //Table data
                int occurances = dataList.Where(x => x.SignalID + ": " + x.Name == s).Count();
                WatchdogDTO tableItem = sortedList.First(x => x.SignalID + ": " + x.Name == s);
                tableItem.Occurrences = occurances;
                tableDataList.Add(tableItem);
            }

            //Plotly plots items in reverse for some reason
            heatmap.Y.Reverse();
            heatmap.Z.Reverse();
            heatmap.TableData = tableDataList;
            List<WatchdogHeatmapDTO> listHeatmap = new List<WatchdogHeatmapDTO>();
            listHeatmap.Add(heatmap);
            return listHeatmap;
        }
    }
}
