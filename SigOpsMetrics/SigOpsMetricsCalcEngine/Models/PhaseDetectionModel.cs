namespace SigOpsMetricsCalcEngine.Models;

public class PhaseDetectionModel
{
    public DateTime timestamp { get; set; }
    public int signalid { get; set; }
    public int deviceid { get; set; }
    public int eventcode { get; set; }
    public int eventparam { get; set; }
    public DateTime date { get; set; }
}