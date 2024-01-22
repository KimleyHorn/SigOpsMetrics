namespace SigOpsMetrics.Models
{
    public class BaseEventLogModel 
    {
        public DateTime Timestamp { get; set; }
        public long? SignalID { get; set; }
        public long? EventCode { get; set; }
        public long? EventParam { get; set; }


        public BaseEventLogModel()
        {
            Timestamp = default;
            SignalID = 0;
            EventCode = 0;
            EventParam = 0;
        }

        public BaseEventLogModel(DateTime timestamp, long signalId, long eventCode, long eventParam)
        {
            Timestamp = timestamp; 
            SignalID = signalId;
            EventCode = eventCode;
            EventParam = eventParam;
        }
    }

}
