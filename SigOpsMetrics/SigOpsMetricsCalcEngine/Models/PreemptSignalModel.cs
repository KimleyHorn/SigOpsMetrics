using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigOpsMetricsCalcEngine.Models
{
    public class PreemptSignalModel
    {
        public DateTime? Timestamp { get; set; }
        public long? SignalID { get; set; }
        public long? EventCode { get; set; }
        public long? EventParam { get; set; }


    }
}
