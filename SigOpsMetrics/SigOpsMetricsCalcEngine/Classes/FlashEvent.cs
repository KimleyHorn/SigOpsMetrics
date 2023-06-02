using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigOpsMetricsCalcEngine.Classes
{
    public class FlashEvent : SigOpsMetrics.API.Classes.DTOs.SignalDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }

        public Point Location { get; set; }
        public FlashEvent(DateTime startTime, DateTime endTime, TimeSpan duration)
        {
            StartTime = startTime;
            EndTime = endTime;
            Duration = duration;
        }

        public FlashEvent()
        {
            StartTime = new DateTime();
            EndTime = new DateTime();
            Duration = new TimeSpan();
            Location = new Point();

        }
    }
}
