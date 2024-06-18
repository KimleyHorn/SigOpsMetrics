using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace SigOpsMetricsCalcEngine.Models
{
    internal class CycleModel : IMetricModel
    {
        public long? SignalID { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration { get; set; }

        public CycleModel()
        {
            SignalID = 0;
            StartTime = default;
            EndTime = default;
            Duration = default;
        }

        public CycleModel(long? signalID, DateTime startTime, DateTime endTime)
        {
            SignalID = signalID;
            StartTime = startTime;
            EndTime = endTime;
            SetDuration();
        }
        public void SetDuration()
        {
            if (StartTime != default && EndTime != default) Duration = EndTime.Value - StartTime.Value;
            else Duration = TimeSpan.Zero;
            
        }

        public override string ToString()
        {
            return $"Cycle Time of {Duration} for {SignalID}" +
                   $"Start Time: {StartTime}" +
                   $"End Time: {EndTime}";
        }
    }
}
