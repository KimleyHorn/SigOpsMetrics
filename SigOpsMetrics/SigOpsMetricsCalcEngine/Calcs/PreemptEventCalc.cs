using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class PreemptEventCalc
    {

        private static List<PreemptSignalModel> _events = new();

        public static async Task CalcPreemptEvent()
        {
            if (_events.Count == 0)
            {
                _events = await PreemptEventDataAccessLayer.ReadAllFromMySql();
            }
            var PreemptList = new List<PreemptModel>();
            var inputOn = _events.Where(x => x.EventCode is 102).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            var entryStart = _events.Where(x => x.EventCode is 105).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            var trackClear = _events.Where(x => x.EventCode is 106).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            var externalCallOn = _events.Where(x => x.EventCode is 707).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            var externalCallOff = _events.Where(x => x.EventCode is 708).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            var inputOff = _events.Where(x => x.EventCode is 104).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            var dwellService= _events.Where(x => x.EventCode is 107).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            var exitCall = _events.Where(x => x.EventCode is 107).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            foreach (var signal in inputOn)
            {
                var signalId = signal.Key;
                var inputOnEvents = signal.ToList();
                var entryStartEvents = entryStart.FirstOrDefault(x => x.Key == signalId)?.ToList();
                var trackClearEvents = trackClear.FirstOrDefault(x => x.Key == signalId)?.ToList();
                var externalCallOnEvents = externalCallOn.FirstOrDefault(x => x.Key == signalId)?.ToList();
                var externalCallOffEvents = externalCallOff.FirstOrDefault(x => x.Key == signalId)?.ToList();
                var inputOffEvents = inputOff.FirstOrDefault(x => x.Key == signalId)?.ToList();
                var dwellServiceEvents = dwellService.FirstOrDefault(x => x.Key == signalId)?.ToList();
                var exitCallEvents = exitCall.FirstOrDefault(x => x.Key == signalId)?.ToList();

                if (entryStartEvents == null || inputOffEvents == null || dwellServiceEvents == null || exitCallEvents == null || externalCallOnEvents == null || externalCallOffEvents == null || trackClearEvents == null) continue;
                foreach (var inputOnEvent in inputOnEvents)
                {
                    //Grabs EventParam from startFlashEvent instead of first from group
                    var preemptType = inputOnEvent.EventParam switch
                    {
                        3 => "EVP",
                        7 => "Flush Preempt",
                        1 => "Railroad",
                        _ => ""
                    };
                    var externalOn = false;
                    var externalOff = false;
                    var entryStartEvent = entryStartEvents.FirstOrDefault(x => x.Timestamp > inputOnEvent.Timestamp);
                    var externalCallOnEvent = externalCallOnEvents.FirstOrDefault(x => x.Timestamp > inputOnEvent.Timestamp);
                    var externalCallOffEvent = externalCallOffEvents.FirstOrDefault(x => x.Timestamp > inputOnEvent.Timestamp);
                    var trackClearEvent = trackClearEvents.FirstOrDefault(x => x.Timestamp > inputOnEvent.Timestamp);
                    var inputOffEvent = inputOffEvents.FirstOrDefault(x => x.Timestamp > inputOnEvent.Timestamp);
                    var dwellServiceEvent = dwellServiceEvents.FirstOrDefault(x => x.Timestamp > inputOnEvent.Timestamp);
                    var exitCallEvent = exitCallEvents.FirstOrDefault(x => x.Timestamp > inputOnEvent.Timestamp);

                    if (entryStartEvent == null) {
                        entryStartEvents.Remove(entryStartEvent!);
                        continue;
                    }

                    if (externalCallOnEvent != null)
                    {
                        externalOn = true;
                    }

                    if (externalCallOffEvent != null)
                    {
                        externalOff = true;
                    }

                    if (inputOffEvent == null)
                    {
                        inputOffEvents.Remove(inputOffEvent!);
                        continue;
                    }

                    if (dwellServiceEvent == null)
                    {
                        dwellServiceEvents.Remove(dwellServiceEvent!);
                        continue;
                    }

                    if (exitCallEvent == null)
                    {
                        exitCallEvents.Remove(exitCallEvent!);
                        continue;
                    }

                    if (trackClearEvent == null)
                    {
                        trackClearEvents.Remove(trackClearEvent!);
                        continue;
                    }

                    var preempt = new PreemptModel(inputOnEvent.Timestamp,inputOffEvent.Timestamp,entryStartEvent.Timestamp,trackClearEvent.Timestamp,dwellServiceEvent.Timestamp,exitCallEvent.Timestamp,signalId, preemptType, externalOff, externalOn);
                    
                    PreemptList.Add(preempt);
                    Console.WriteLine(preempt.ToString());
                }
            }
            await PreemptEventDataAccessLayer.WritePreemptEventsToDB(PreemptList);
        }

    }
}
