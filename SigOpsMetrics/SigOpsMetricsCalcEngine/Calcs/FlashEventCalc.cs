using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Parquet;
using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;
using System.Configuration;
using Parquet.Schema;
using Parquet.Thrift;

namespace SigOpsMetricsCalcEngine.Calcs
{
    public class FlashEventCalc
    {
        private static List<FlashEventModel> _events = new();


        public static async Task CalcFlashEvent()
        {
            if (_events.Count == 0)
            {
                _events = await FlashEventDataAccessLayer.ReadAllFromMySql();
            }
            var flashPairList = new List<FlashPairModel>();
            var startFlash = _events.Where(x => x.EventParam is 4 or 7).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            var endFlash = _events.Where(x => x.EventParam == 2).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            foreach (var signal in startFlash)
            {
                var signalId = signal.Key;
                var startFlashEvents = signal.ToList();
                var endFlashEvents = endFlash.FirstOrDefault(x => x.Key == signalId)?.ToList();
                if (endFlashEvents == null) continue;
                foreach (var startFlashEvent in startFlashEvents)
                {
                    //Grabs EventParam from startFlashEvent instead of first from group
                    var startParam = startFlashEvent.EventParam;

                    var endFlashEvent = endFlashEvents.FirstOrDefault(x => x.Timestamp > startFlashEvent.Timestamp);
                    
                    if (endFlashEvent == null)
                    {
                        endFlashEvents.Remove(endFlashEvent!);
                        continue;
                    }
                    
                    var flashPair = new FlashPairModel(startFlashEvent, endFlashEvent, signalId, startParam);
                    endFlashEvents.Remove(endFlashEvent);
                    flashPairList.Add(flashPair);
                    Console.WriteLine(flashPair.ToString());
                }
            }
            await FlashEventDataAccessLayer.WriteFlashPairsToDb(flashPairList);

        }

    }
}