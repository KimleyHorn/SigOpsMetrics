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
        public string month { get; set; }
        public int? dateRange { get; set; }
        /// <summary>
        /// Custom start date
        /// </summary>
        public string customStart { get; set; }
        /// <summary>
        /// Custom end date
        /// </summary>
        public string customEnd { get; set; }
        /// <summary>
        /// List of selected days (Mo, Tu, We, Th, Fr, Sa, Su)
        /// </summary>
        public string[]? daysOfWeek { get; set; }
        /// <summary>
        /// Custom start time
        /// </summary>
        public string startTime { get; set; }
        /// <summary>
        /// Custom end time
        /// </summary>
        public string endTime { get; set; }
        /// <summary>
        /// Numeric value for time aggregation (min, hour, daily, weekly, monthly, quarterly)
        /// </summary>
        public int? timePeriod { get; set; }
        /// <summary>
        /// Zone group/Region
        /// </summary>
        public string zone_Group { get; set; }
        /// <summary>
        /// Zone/District
        /// </summary>
        public string zone { get; set; }
        /// <summary>
        /// Agency
        /// </summary>
        public string agency { get; set; }
        /// <summary>
        /// Location County
        /// </summary>
        public string county { get; set; }
        /// <summary>
        /// Location city
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// Location corridor
        /// </summary>
        public string corridor { get; set; }
        /// <summary>
        /// Location subcorridor
        /// </summary>
        public string subcorridor { get; set; }
    }
}
