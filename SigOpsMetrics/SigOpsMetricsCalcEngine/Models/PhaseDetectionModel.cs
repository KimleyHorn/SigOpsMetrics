namespace SigOpsMetricsCalcEngine.Models;

public class PhaseDetectionModel
{
    public DateTime Date { get; set; }
    public long? SignalID { get; set; }
    public float? Downtime { get; set; }
}