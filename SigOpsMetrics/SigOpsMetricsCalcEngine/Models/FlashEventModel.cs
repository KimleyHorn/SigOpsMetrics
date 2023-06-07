using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SigOpsMetricsCalcEngine.Models
{
    public class FlashEventModel
    {

        public DateTime? Timestamp { get; set; }
        public long? SignalID { get; set; }
        public long? EventCode { get; set; }
        public long? EventParam { get; set; }
        public long? DeviceID { get; set; }
        public long? __index_level_0__ { get; set; }


    }
}
