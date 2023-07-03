using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3.Model;
using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class PreemptEventCalc
    {

        private static List<BaseSignalModel> _events = new();
        private static async Task GetEvents()
        {
            if (BaseDataAccessLayer.SignalEvents.Count == 0)
            {
                _events = await PreemptEventDataAccessLayer.ReadAllFromMySql();
            }
            else
            {
                _events = BaseDataAccessLayer.SignalEvents;
            }
        }


        public static async Task CalcPreemptEvent()
        {
            await GetEvents();

            var preemptList = new List<PreemptModel>();
            var inputOn = FilterByEventCode(102);
            var entryStart = FilterByEventCode(105);
            var trackClear = FilterByEventCode(106);
            var externalCallOn = FilterByEventCode(707);
            var externalCallOff = FilterByEventCode(708);
            var inputOff = FilterByEventCode(104);
            var dwellService = FilterByEventCode(107);
            var exitCall = FilterByEventCode(111);
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

                }

            }

            await PreemptEventDataAccessLayer.WritePreemptEventsToDb(preemptList);
        }
        #region Helper Methods


        private static List<BaseSignalModel> FilterByEventCode(long eventCode)
        {
            return _events.Where(x => x.EventCode == eventCode).OrderBy(x => x.Timestamp).ToList();
        }

        private static BaseSignalModel? FirstOrDefaultList(IEnumerable<BaseSignalModel>? events, DateTime Timestamp, long? signalID)
        {
            return  events?.FirstOrDefault(x => x.Timestamp >= Timestamp && x.SignalID == signalID);
        }

        #endregion
    }
}
