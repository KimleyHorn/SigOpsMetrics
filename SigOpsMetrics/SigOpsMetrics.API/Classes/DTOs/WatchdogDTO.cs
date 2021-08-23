using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SigOpsMetrics.API.Classes.DTOs
{
    public class WatchdogDTO
    {
        public string ZoneGroup { get; set; }
        public string Zone { get; set; }
        public string Corridor { get; set; }
        public string SignalID { get; set; }
        public string Name { get; set; }
        public string Alert { get; set; }
        public int Occurrences { get; set; }
        public int Streak { get; set; }
        public string Date { get; set; }
    }

    public class WatchdogFilterRequestObject
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Alert { get; set; }
        public string Phase { get; set; }
        public string IntersectionFilter { get; set; }
        public string Streak { get; set; }
    }

    public class WatchdogHeatmapDTO {
        public List<List<int?>> Z { get; set; }
        public List<string> Y { get; set; }
        public List<string> X { get; set; }
        public List<WatchdogDTO> TableData { get; set; }
    }

}
