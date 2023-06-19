using System;

namespace SigOpsMetrics.API.Classes.DTOs
{
    public class FlashEventDTO
    {

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public long? SignalID { get; set; }
        public long? duration { get; set; }
        public long? startParam { get; set; }

    }
}
