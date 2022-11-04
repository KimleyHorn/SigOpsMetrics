using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SigOpsMetrics.API.Classes
{
    public class Corridor
    {
        public string CorridorId { get; set; }

        /// <summary>
        /// The time interval that data can be grouped by
        /// </summary>
        /// <example>
        /// Month, Week
        /// </example>        
        public DateTime TimePeriod { get; set; }
        
        /// <summary>
        /// This is the field that gets calculated/averaged
        /// </summary>
        /// <example>
        /// VPH, PD
        /// </example>
        public double CalculatedField { get; set; }
        public double Ones { get; set; }
        public double Delta { get; set; }
        public string ZoneGroup { get; set; }
        public string Description { get; set; }

        public IEnumerable<Signal> Signals { get; set; } = Enumerable.Empty<Signal>();
    }
}
