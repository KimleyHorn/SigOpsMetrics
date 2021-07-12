using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SigOpsMetrics.API.Classes.DTOs
{
    public class AverageDTO
    {
        public string label { get; set; }
        public double avg { get; set; }
        public double delta { get; set; }
    }
}
