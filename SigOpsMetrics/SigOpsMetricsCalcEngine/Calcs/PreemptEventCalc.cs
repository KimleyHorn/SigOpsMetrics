using System.Configuration;
using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class PreemptEventCalc
    {
        private static readonly string MySqlTableName = ConfigurationManager.AppSettings["PREEMPT_EVENT_TABLE_NAME"] ?? "preempt_event_log";
        private static readonly string MySqlDbName = ConfigurationManager.AppSettings["DB_NAME"] ?? "test";

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
                        entryStart.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);

                    var externalCallOnEvent =
                        externalCallOn.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);
                    var externalCallOffEvent =
                        externalCallOff.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);
                    var trackClearEvent = trackClear.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);
                    var inputOffEvent = inputOff.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);
                    var dwellServiceEvent = dwellService.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);
                    var exitCallEvent = exitCall.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);


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
                        await BaseDataAccessLayer.WriteToErrorLog("PreemptEventCalc", "CalcPreemptEvents", e);
                        return false;

                }

            }

            return await PreemptEventDataAccessLayer.WritePreemptEventsToDb(preemptList);
        }

        private static List<BaseEventLogModel> FilterByEventCode(List<BaseEventLogModel> events, long eventCode)
        {
            return events.Where(x => x.EventCode == eventCode).OrderBy(x => x.Timestamp).ToList();
        }


        public static async Task<bool> RunPreempt(DateTime startDate, DateTime endDate)
        {
            //var dateList = Enumerable.Range(0, (int)(endDate - startDate).TotalDays + 1)
            //                               .Select(offset => startDate.AddDays(offset))
            //                               .ToList();
            if (endDate < startDate)
                throw new ArgumentException("Start date must be before end date");


            await BaseDataAccessLayer.CheckDB(MySqlTableName, "Timestamp", MySqlDbName, startDate, endDate);

            //Preempt event list to check for eventCodes from SignalEvents list
            var preemptEventList = new List<long?> { 102, 105, 106, 104, 107, 111, 173, 707, 708 };
            var validDates = BaseDataAccessLayer.FillData(startDate, endDate, preemptEventList);


            if (BaseDataAccessLayer.HasGaps(validDates))
            {
                Console.WriteLine("Gaps in data");
                var t = await PreemptEventDataAccessLayer.ProcessPreemptSignals(validDates, eventCodes: preemptEventList);
                PreemptEventDataAccessLayer.ConvertToPreempt(startDate, endDate);
                return await PreemptEventDataAccessLayer.WritePreemptSignalsToDb(t);

            }
            //This checks to see if the events are already in the database or if they are already stored in memory
            if (BaseDataAccessLayer.CheckList(startDate, endDate, preemptEventList) ||
                await BaseDataAccessLayer.CheckDB("preempt_log", "Timestamp", "mark1", startDate, endDate))
                return await CalcPreemptEvent(BaseDataAccessLayer.PreemptEvents());
            //If events within date range are not within the database or memory, then grab the events from Amazon S3
            //var t = await PreemptEventDataAccessLayer.ProcessPreemptSignals(validDates, eventCodes: preemptEventList);
            //PreemptEventDataAccessLayer.ConvertToPreempt(startDate, endDate);
            //await PreemptEventDataAccessLayer.WritePreemptSignalsToDb(t);

            //Afterwards convert signal events to preempt events and add them to the preempt event list
            //Calculate the preempt events based on preempt list
            return await CalcPreemptEvent(BaseDataAccessLayer.PreemptEvents());
        }


    }
}
