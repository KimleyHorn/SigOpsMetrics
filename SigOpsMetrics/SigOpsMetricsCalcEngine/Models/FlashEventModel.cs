using System.Diagnostics.CodeAnalysis;

namespace SigOpsMetricsCalcEngine.Models
{
    public class FlashEventModel : BaseSignalModel
    {

        public new DateTime Timestamp { get; set; }
#pragma warning disable IDE0079 // Remove unnecessary suppression
        [SuppressMessage("ReSharper", "InconsistentNaming")] public long? SignalID { get; set; }
        public new long? EventCode { get; set; }
        public new long? EventParam { get; set; }
#pragma warning restore IDE0079 // Remove unnecessary suppression

        public override string ToString()
        {
            return $"Flash Event Signal for Sensor {SignalID}: {Timestamp} with Event Parameter {EventParam}";
        }


    }
}
