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
        /// <summary>
        /// Deprecated
        /// </summary>
        /// <example>null</example>
        public string month { get; set; }

        /// <summary>
        /// 0: Prior Day<br />
        /// 1: Prior Week<br />
        /// 2: Prior Month<br />
        /// 3: Prior Quarter<br />
        /// 4: Prior Year<br />
        /// 5: Custom (see customStart, customEnd, startTime, and endTime)
        /// </summary>
        /// <example>4</example>
        public int? dateRange { get; set; }

        /// <summary>
        /// When a custom dateRange is specified (dateRange = 5), this contains the custom start date to be used
        /// </summary>
        /// <example>null</example>
        public string customStart { get; set; }

        /// <summary>
        /// When a custom dateRange is specified (dateRange = 5), this contains the custom end date to be used
        /// </summary>
        /// <example>null</example>
        public string customEnd { get; set; }

        /// <summary>
        /// Deprecated 
        /// </summary>
        /// <example>null</example>
        public string[]? daysOfWeek { get; set; }

        /// <summary>
        /// When a custom dateRange is specified (dateRange = 5), this contains the custom start time to be used
        /// </summary>
        /// <example>null</example>
        public string startTime { get; set; }

        /// <summary>
        /// When a custom dateRange is specified (dateRange = 5), this contains the custom end time to be used
        /// </summary>
        /// <example>null</example>
        public string endTime { get; set; }

        /// <summary>
        /// Data is aggregated based on the value supplied:<br />
        /// 0: Quarter-hourly<br />
        /// 1: Hourly<br />
        /// 2: Daily<br />
        /// 3: Weekly<br />
        /// 4: Monthly<br />
        /// 5: Quarterly<br />
        /// </summary>
        /// <example>4</example>
        public int? timePeriod { get; set; }

        /// <summary>
        /// Zone group/Region filter<br />
        /// Values are subject to change, but current values include:<br />
        /// All<br />
        /// Central Metro<br />
        /// East Metro<br />
        /// North<br />
        /// Southeast<br />
        /// Southwest<br />
        /// West Metro<br />
        /// </summary>
        /// <example>Central Metro</example>
        public string zone_Group { get; set; }

        /// <summary>
        /// Zone/District filter
        /// </summary>
        /// <example>null</example>
        public string zone { get; set; }

        /// <summary>
        /// Agency filter
        /// </summary>
        /// <example>null</example>
        public string agency { get; set; }

        /// <summary>
        /// County filter
        /// </summary>
        /// <example>null</example>
        public string county { get; set; }

        /// <summary>
        /// City filter
        /// </summary>
        /// <example>null</example>
        public string city { get; set; }

        /// <summary>
        /// Corridor filter
        /// </summary>
        /// <example>null</example>
        public string corridor { get; set; }

        /// <summary>
        /// Signal ID filter - narrows results to a single intersection
        /// </summary>
        /// <example>null</example>
        public string signalId { get; set; }

        /// <summary>
        /// Subcorridor filter
        /// </summary>
        /// <example>null</example>
        public string subcorridor { get; set; }
        
        /// <summary>
        /// Priority filter
        /// </summary>
        /// <example>null</example>
        public string priority { get; set; }

        /// <summary>
        /// Classification filter
        /// </summary>
        /// <example>null</example>
        public string classification { get; set; }
    }
}
