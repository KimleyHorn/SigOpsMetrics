using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigOpsMetricsCalcEngine.Models
{
    internal interface IMetricModel
    { 
        long? SignalID { get; set; }
        DateTime? StartTime { get; set; }
        DateTime? EndTime { get; set; }
        TimeSpan Duration { get; set; }

        public void SetDuration();
        public string? ToString();
    }
}
