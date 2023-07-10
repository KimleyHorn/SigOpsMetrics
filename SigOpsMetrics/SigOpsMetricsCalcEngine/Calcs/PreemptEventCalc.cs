using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class PreemptEventCalc
    {


        public static async Task<bool> CalcPreemptEvent(List<BaseEventLogModel> baseSignal)
        {


            var preemptList = new List<PreemptModel>();
            var inputOn = FilterByEventCode(baseSignal, 102);
            var entryStart = FilterByEventCode(baseSignal,105);
            var trackClear = FilterByEventCode(baseSignal, 106);
            var externalCallOn = FilterByEventCode(baseSignal, 707);
            var externalCallOff = FilterByEventCode(baseSignal, 708);
            var inputOff = FilterByEventCode(baseSignal, 104);
            var dwellService = FilterByEventCode(baseSignal, 107);
            var exitCall = FilterByEventCode(baseSignal, 111);
            foreach (var signal in inputOn)
            {

                //TODO change to Where
                var signalId = signal.SignalID;

                try
                {
                    //Grabs EventParam from startFlashEvent instead of first from group
                    var preemptType = signal.EventParam switch
                    {
                        3 => "EVP",
                        7 => "Flush Preempt",
                        1 => "Railroad",
                        _ => "Default"
                    };
                    var externalOn = false;
                    var externalOff = false;

                    var entryStartEvent =
                        FirstOrDefaultList(entryStart, signal.Timestamp, signalId);

                    var externalCallOnEvent =
                        FirstOrDefaultList(externalCallOn, signal.Timestamp, signalId);
                    var externalCallOffEvent =
                        FirstOrDefaultList(externalCallOff, signal.Timestamp, signalId);
                    var trackClearEvent = FirstOrDefaultList(trackClear, signal.Timestamp, signalId);
                    var inputOffEvent = FirstOrDefaultList(inputOff, signal.Timestamp, signalId);
                    var dwellServiceEvent = FirstOrDefaultList(dwellService, signal.Timestamp, signalId);
                    var exitCallEvent = FirstOrDefaultList(exitCall, signal.Timestamp, signalId);


                    if (externalCallOnEvent != null)
                    {
                        externalOn = true;
                    }

                    if (externalCallOffEvent != null)
                    {
                        externalOff = true;
                    }

                    var preempt = new PreemptModel(signal.Timestamp, inputOffEvent?.Timestamp,
                        entryStartEvent?.Timestamp, trackClearEvent?.Timestamp, dwellServiceEvent?.Timestamp,
                        exitCallEvent?.Timestamp, signalId, preemptType, externalOff, externalOn);

                    preemptList.Add(preempt);

                    Console.WriteLine(preempt.ToString());
                }
                catch (Exception e) 
                {
                        Console.WriteLine(e);
                        return false;

                }

            }

            return await PreemptEventDataAccessLayer.WritePreemptEventsToDb(preemptList);
        }
        #region Helper Methods


        private static List<BaseEventLogModel> FilterByEventCode(List<BaseEventLogModel> events, long eventCode)
        {
            return events.Where(x => x.EventCode == eventCode).OrderBy(x => x.Timestamp).ToList();
        }

        private static BaseEventLogModel? FirstOrDefaultList(IEnumerable<BaseEventLogModel>? events, DateTime Timestamp, long? signalID)
        {
            return  events?.FirstOrDefault(x => x.Timestamp >= Timestamp && x.SignalID == signalID);
        }

        #endregion

        public static async Task<bool> RunPreempt(DateTime startDate, DateTime endDate)
        {
            //Preempt event list to check for eventCodes from SignalEvents list
            var preemptEventList = new List<long?> { 102, 105, 106, 104, 107, 111, 173, 707, 708 };

            //This checks to see if the events are already in the database or if they are already stored in memory
            if (BaseDataAccessLayer.CheckList(startDate, endDate, preemptEventList) ||
                await BaseDataAccessLayer.CheckDB("preempt_log", "Timestamp", "mark1", startDate, endDate))
                return await CalcPreemptEvent(BaseDataAccessLayer.PreemptEvents);
            //If events within date range are not within the database or memory, then grab the events from Amazon S3
            var t = await PreemptEventDataAccessLayer.ProcessPreemptSignals(startDate, endDate, eventCodes: preemptEventList);
            PreemptEventDataAccessLayer.ConvertToPreempt(startDate, endDate);
            await PreemptEventDataAccessLayer.WritePreemptSignalsToDb(t);

            //Afterwards convert signal events to preempt events and add them to the preempt event list
            //Calculate the preempt events based on preempt list
            return await CalcPreemptEvent(BaseDataAccessLayer.PreemptEvents);
        }


    }
}
