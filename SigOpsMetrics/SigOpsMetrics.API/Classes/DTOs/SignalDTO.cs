using System;

namespace SigOpsMetrics.API.Classes.DTOs
{
    public class SignalDTO
    {
        public int SignalID { get; set; }
        public string ZoneGroup { get; set; }
        public string Zone { get; set; }
        public string Corridor { get; set; }
        public string Subcorridor { get; set; }
        public string Agency { get; set; }
        public string MainStreetName { get; set; }
        public string SideStreetName { get; set; }
        public decimal Milepost { get; set; }
        public DateTime? AsOf { get; set; }
        public int Duplicate { get; set; }
        public bool Include { get; set; }
        public DateTime? Modified { get; set; }
        public string Note { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string County { get; set; }
        public string City { get; set; }

    }
}
