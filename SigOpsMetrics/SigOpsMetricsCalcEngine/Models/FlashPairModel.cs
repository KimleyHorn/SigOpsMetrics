namespace SigOpsMetricsCalcEngine.Models
{
    public class FlashPairModel 
    {
      public long? SignalID{ get; set; }
      public DateTime FlashStart { get; set; }
      public DateTime? FlashEnd { get; set; }
      public TimeSpan FlashDuration { get; set; }
      public long? StartParam { get; set; }

      public bool IsOpen { get; set; }


        public FlashPairModel(){
            SignalID = 0;
            FlashStart = default;
            FlashEnd = default;
            FlashDuration = default;
            StartParam = 0;
            IsOpen = true;
        }
   



        public FlashPairModel(FlashEventModel flashStart, FlashEventModel flashEnd, long? signalId, long? startParam, bool isOpen)
        {
            SignalID = signalId;
            StartParam = startParam;
            FlashStart = flashStart.Timestamp;
            FlashEnd = flashEnd.Timestamp;
            IsOpen = isOpen;
            SetFlashDuration();
            
        }

        public FlashPairModel(long? startParam, long? signalId = 0)
        {
            StartParam = startParam;
            StartParam = signalId;
            FlashStart = default; 
            FlashEnd = default;
            IsOpen = true;
            SetFlashDuration();
        }

        public void SetFlashDuration()
        {
            if (FlashEnd == null)
            {
                FlashDuration = (DateTime.Now - FlashStart);
                return;
            }
            FlashDuration = ((DateTime)FlashEnd - FlashStart);
        } 
        public override string ToString()
        {

            if(FlashEnd == null)
                return $"Open flash Event for Sensor {SignalID}";


            return $"Flash Event for Sensor {SignalID}: Lasted {TimeSpan.FromTicks(FlashDuration.Ticks)} between {FlashStart} and {FlashEnd}";
        }
    }
}
