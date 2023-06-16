using System.Diagnostics.CodeAnalysis;

namespace SigOpsMetricsCalcEngine.Models
{
    public class FlashEventModel
    {

        public DateTime Timestamp { get; set; }
#pragma warning disable IDE0079 // Remove unnecessary suppression
        [SuppressMessage("ReSharper", "InconsistentNaming")] public long? SignalID { get; set; }
        public long? EventCode { get; set; }
        public long? EventParam { get; set; }

        [SuppressMessage("ReSharper", "InconsistentNaming")] public long? DeviceID { get; set; }

#pragma warning disable IDE1006 // Naming Styles
        [SuppressMessage("ReSharper", "InconsistentNaming")] public long? __index_level_0__ { get; set; }
        #pragma warning restore IDE1006 // Naming Styles
        #pragma warning restore IDE0079 // Remove unnecessary suppression

        public override string ToString()
        {
            return $"Flash Event for Sensor {SignalID}: {Timestamp} with Event Parameter {EventParam}";
        }


    }
}
