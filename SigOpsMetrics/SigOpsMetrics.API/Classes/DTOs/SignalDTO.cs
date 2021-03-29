using System;

namespace SigOpsMetrics.API.Classes.DTOs
{
    public class SignalDTO
    {
        public string SignalID { get; set; }
        public string ZoneGroup { get; set; }
        public string Zone { get; set; }
        public string Corridor { get; set; }
        public string Subcorridor { get; set; }
        public string Agency { get; set; }
        public string MainStreetName { get; set; }
        public string SideStreetName { get; set; }
        public string Milepost { get; set; }
        public DateTime? AsOf { get; set; }
        public string Duplicate { get; set; }
        public string Include { get; set; }
        public DateTime? Modified { get; set; }
        public string Note { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
