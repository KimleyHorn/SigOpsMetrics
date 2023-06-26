using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigOpsMetricsCalcEngine.Models
{
    public interface ISignalModel
    {
        DateTime Timestamp { get; set; }
        long? SignalID { get; set; }
        long? EventCode { get; set; }
        long? EventParam { get; set; }


    }
}
