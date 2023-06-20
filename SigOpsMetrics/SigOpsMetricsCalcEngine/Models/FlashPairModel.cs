namespace SigOpsMetricsCalcEngine.Models
{
    public class FlashPairModel 
    {
      public long? SignalId{ get; set; }
      public FlashEventModel FlashStart { get; set; }
      public FlashEventModel? FlashEnd { get; set; }
      public TimeSpan FlashDuration { get; set; }
      public long? StartParam { get; set; }

      public bool IsOpen { get; set; }


        
   



        public FlashPairModel(FlashEventModel flashStart, FlashEventModel flashEnd, long? signalId, long? startParam, bool isOpen)
        {
            SignalId = signalId;
            StartParam = startParam;
            FlashStart = flashStart;
            FlashEnd = flashEnd;
            IsOpen = isOpen;
            SetFlashDuration();
            
        }

        public FlashPairModel(long? startParam, long? signalId = 0)
        {
            StartParam = startParam;
            StartParam = signalId;
            FlashStart = new FlashEventModel(); 
            FlashEnd = new FlashEventModel();
            IsOpen = true;
            SetFlashDuration();
        }

        public void SetFlashDuration()
        {
            if (FlashEnd == null)
            {
                FlashDuration = (DateTime.Now - FlashStart.Timestamp);
                return;
            }
            FlashDuration = (FlashEnd.Timestamp - FlashStart.Timestamp);
        } 
        public override string ToString()
        {

            if(FlashEnd == null)
                return $"Open flash Event for Sensor {SignalId}";


            return $"Flash Event for Sensor {SignalId}: Lasted {TimeSpan.FromTicks(FlashDuration.Ticks)} between {FlashStart.Timestamp} and {FlashEnd.Timestamp}";
        }
    }
}
