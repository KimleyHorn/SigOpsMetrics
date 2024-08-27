using System;
using System.ComponentModel;
using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;
using MySqlConnector;
using Parquet;
using SigOpsMetricsCalcEngine.Models;
using System.Configuration;
using System.Data;
using System.Security.Policy;

public class PhaseDetectionDataAccessLayer : BaseDataAccessLayer
{
    public PhaseDetectionDataAccessLayer(List<BaseEventLogModel> sigModels)
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
        var filteredSignals = new List<BaseEventLogModel>();
        filteredSignals = events.Where(x =>
        {
            var ignoreSignals = new List<BaseEventLogModel>();
            if (ignoreSignals.Contains(x) || x.EventCode == 46) return false;
            if (x.Timestamp.Hour >= 7 && x.Timestamp.Hour <= 17)
            {
                if (x.EventCode.Equals(0))
                {
                    var nextSignalEvent = events.FirstOrDefault(e =>
                        (e.SignalID == x.SignalID) && (e.EventParam == x.EventParam));
                    if (nextSignalEvent.EventCode.Equals(46))
                    {
                        ignoreSignals.Add(nextSignalEvent);
                        return false;
                    }
                }
                return true;
            }
            return false;
        }).ToList();
        return filteredSignals;
    }
}
