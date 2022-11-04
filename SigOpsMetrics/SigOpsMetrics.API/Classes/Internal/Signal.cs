using Microsoft.VisualBasic;

namespace SigOpsMetrics.API.Classes
{
    public class Signal
    {
        public string SignalId { get; set; }
        public string Corridor { get; set; }
        public DateAndTime Month { get; set; }
        public double VPH { get; set; } 
        public double Delta { get; set; }
        public string ZoneGroup { get; set; }
        public string Description { get; set; }
    }
}
