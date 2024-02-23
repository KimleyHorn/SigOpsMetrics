
namespace SigOpsMetricsCalcEngine.Models
{
    public class PreemptModel
    {


        public DateTime? InputOn { get; set; }
        public DateTime? InputOff { get; set; }
        public DateTime? EntryStart { get; set; }
        public DateTime? TrackClear { get; set; }
        public DateTime? DwellService { get; set; }
        public DateTime? ExitCall { get; set; }
        public long? SignalID { get; set; }

        public bool? ExternalCallOff { get; set; }
        public bool? ExternalCallOn { get; set; }

        public TimeSpan Duration { get; set; }

        public string? PreemptType { get; set; }

        public void SetDuration()
        {
            if (InputOn != null && ExitCall != null)
            {
                Duration = ExitCall.Value - InputOn.Value;
            }
            else
            {
                Duration = TimeSpan.Zero;
            }
        }

        public PreemptModel()
        {
            InputOn = default;
            InputOff = default;
            EntryStart = default;
            TrackClear = default;
            DwellService = default;
            ExitCall = default;
            SignalID = 0;
            Duration = default;
            ExternalCallOn = false;
            ExternalCallOff = false;
            PreemptType = "";
        }

        public PreemptModel(DateTime? inputOn, DateTime? inputOff, DateTime? entryStart, DateTime? trackClear, DateTime? dwellService, DateTime? exitCall, long? signalId, string? preemptType, bool? externalCallOff = false, bool? externalCallOn = false)
        {
            InputOn = inputOn;
            InputOff = inputOff;
            EntryStart = entryStart;
            TrackClear = trackClear;
            DwellService = dwellService;
            ExitCall = exitCall;
            SignalID = signalId; 
            SetDuration();
            ExternalCallOff = externalCallOff;
            ExternalCallOn = externalCallOn;
            PreemptType = preemptType;
        }

        public override string ToString()
        {
            return $"{PreemptType} Preempt for Signal {SignalID}, " + 
                   $"Duration: {Duration}, " +
                   $"Start time: {InputOn?.Date}, " +
                   $@"End time: {ExitCall?.Date}," + 
                   $@"External Preempt Call On: {ExternalCallOn}, " +
                   $@"External Call Off: {ExternalCallOff}, ";
        }
    }
}
