namespace SigOpsMetricsCalcEngine.Core.Models;

public class PhaseDetectionModel
{
    public DateTime Date { get; set; }
    public long? SignalID { get; set; }
    public long? Phase { get; set; }
    public float? Downtime { get; set; }
}