using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming
#pragma warning disable 1591

namespace SigOpsMetrics.API.Classes.DTOs
{
    /// <summary>
    /// Matches filter.ts file in front-end
    /// </summary>
    public class FilterDTO
    {
        public string zone_Group { get; set; }
        public string corridor { get; set; }
        public string month { get; set; }
        public string zone { get; set; }
        public string agency { get; set; }
        public string county { get; set; }
        public string city { get; set; }
        public int timePeriod { get; set; }
        public DateTime customStart { get; set; }
        public DateTime customEnd { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
    }
}
