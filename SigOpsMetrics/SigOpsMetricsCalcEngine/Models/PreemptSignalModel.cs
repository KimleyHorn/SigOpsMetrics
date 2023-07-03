namespace SigOpsMetricsCalcEngine.Models
{
    public class PreemptSignalModel : BaseSignalModel
    {
        //public new DateTime Timestamp { get; set; }
        //public new long? SignalID { get; set; }
        //public new long? EventCode { get; set; }
        //public new long? EventParam { get; set; }


        public PreemptSignalModel(){
            Timestamp = default;
            base.SignalID = 0;
            EventCode = 0;
            EventParam = 0;
        }

        public PreemptSignalModel(DateTime timestamp, long signalId, long eventCode, long eventParam) {
            Timestamp = timestamp;
            base.SignalID = signalId;
            EventCode = eventCode;
            EventParam = eventParam;
        }
    }
}
