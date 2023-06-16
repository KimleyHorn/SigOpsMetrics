namespace SigOpsMetricsCalcEngine.Models
{
    public class FlashPairModel 
    {
      public long? SignalId{ get; set; }
      public FlashEventModel FlashStart { get; set; }
      public FlashEventModel FlashEnd { get; set; }
      public TimeSpan FlashDuration { get; set; }
      public long? StartParam { get; set; }


        
   



        public FlashPairModel(FlashEventModel flashStart, FlashEventModel flashEnd, long? signalId, long? startParam)
        {
            SignalId = signalId;
            StartParam = startParam;
            FlashStart = flashStart;
            FlashEnd = flashEnd;
            SetFlashDuration();
            
        }

        public FlashPairModel(long? startParam, long? signalId = 0)
        {
            StartParam = startParam;
            StartParam = signalId;
            FlashStart = new FlashEventModel(); 
            FlashEnd = new FlashEventModel();
            SetFlashDuration();
        }

        public void SetFlashDuration() => FlashDuration = (TimeSpan)(FlashEnd.Timestamp - FlashStart.Timestamp);
        public override string ToString()
        {
            return $"Flash Event for Sensor {SignalId}: Lasted {TimeSpan.FromTicks(FlashDuration.Ticks)} between {FlashStart.Timestamp} and {FlashEnd.Timestamp}";
        }
    }
}
