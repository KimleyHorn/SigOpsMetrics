using System;
using System.Collections.Generic;
using System.Linq;

namespace SigOpsMetrics.API.Classes.Internal
{
    public class Corridor
    {
        /// <summary>
        /// The Corridor Id 
        /// </summary>
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

        /// <summary>
        /// This is currently not used but it was added to be consistent with the database and datatables used.
        /// </summary>
        public double Ones { get; set; }

        /// <summary>
        /// The delta for the specific record.
        /// </summary>
        public double Delta { get; set; }

        /// <summary>
        /// The Zone Group or Region.
        /// </summary>
        public string ZoneGroup { get; set; }

        /// <summary>
        /// The description of the Corridor.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// List of all the Signals associated with this Corridor.
        /// </summary>
        public IEnumerable<Signal> Signals { get; set; } = Enumerable.Empty<Signal>();
    }
}
