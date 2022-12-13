using Microsoft.VisualBasic;

namespace SigOpsMetrics.API.Classes.Internal
{
    public class Signal
    {
        /// <summary>
        /// SignalId
        /// </summary>
        public string SignalId { get; set; }

        /// <summary>
        /// CorridorId
        /// </summary>
        public string Corridor { get; set; }

        /// <summary>
        /// Month or Time Period used for the Signal
        /// </summary>
        public DateAndTime Month { get; set; }

        /// <summary>
        /// The VPH or field that is used to calculate data.
        /// </summary>
        public double VPH { get; set; } 

        /// <summary>
        /// Delta of the Signal data.
        /// </summary>
        public double Delta { get; set; }

        /// <summary>
        /// Zone or Region associated with the Signal
        /// </summary>
        public string ZoneGroup { get; set; }

        /// <summary>
        /// Description of the signal
        /// </summary>
        public string Description { get; set; }
    }
}
