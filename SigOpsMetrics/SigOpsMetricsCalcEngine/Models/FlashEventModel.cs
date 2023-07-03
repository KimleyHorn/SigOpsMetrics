using System.Diagnostics.CodeAnalysis;

namespace SigOpsMetricsCalcEngine.Models
{
    public class FlashEventModel : BaseSignalModel
    {

        public override string ToString()
        {
            return $"Flash Event Signal for Sensor {SignalID}: {Timestamp} with Event Parameter {EventParam}";
        }


    }
}
