using System.Collections.Concurrent;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess;

public class PhaseDetectionDataAccessLayer : BaseDataAccessLayer
{
    public PhaseDetectionDataAccessLayer(ConcurrentBag<BaseEventLogModel> sigModels)
    {
        SignalEvents = sigModels;
    }

    /// <summary>
    /// Filters the list of BaseEventLogModel. Designed to filter data down to the timeframe,
    /// will then check whether the eventCode is 0. If the eventCode is 0, it will find the next
    /// signal event where the signal and phase matches the previous signal event. If the next signal event is 46,
    /// the original signal as well as the subsequent will be excluded. All 46 codes will be excluded.
    /// 
    /// --> Will return a list of the "total uptime" signals on the day
    /// </summary>
    /// <param name="events"> List of signal events passed in as a sigModel list in the PhaseDetectionCalc </param>
    ///
    /// <returns></returns>
    public static List<BaseEventLogModel> FilterMissedOrOmitted(List<BaseEventLogModel> events, long? signalID)
    {
        //Brig up issues with parameter
        var signals = new List<BaseEventLogModel>();
        signals = events.Where(x =>
        {
            var ignoreSignals = new List<BaseEventLogModel>();
            if (ignoreSignals.Contains(x) || x.EventCode == 46) return false;
            if (x.Timestamp.Hour is < 7 or > 17) return false;
            if (!x.EventCode.Equals(0)) return true;
            var nextSignalEvent = events.FirstOrDefault(e =>
                (e.SignalID == x.SignalID) && (e.EventParam == x.EventParam));
            if (!nextSignalEvent.EventCode.Equals(46)) return true;
            ignoreSignals.Add(nextSignalEvent);
            return false;
        }).ToList();
        return signals;
    }
}