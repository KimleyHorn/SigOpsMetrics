using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SigOpsMetricsCalcEngine.Models
{
    internal class CycleDashModel
    {
        public int MVID;
        public DateOnly StartDate;
        public TimeOnly Time;
        public int CycleLength;
        public int Pattern;
    }
}
