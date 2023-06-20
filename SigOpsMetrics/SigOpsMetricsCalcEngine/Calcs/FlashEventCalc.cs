using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    public class FlashEventCalc
    {
        private static List<FlashEventModel> _events = new();

        private static async Task GetEvents()
        {
            if (BaseDataAccessLayer.FlashEvents.Count == 0)
            {
                _events = await FlashEventDataAccessLayer.ReadAllFromMySql();
            }
            else
            {
                _events = BaseDataAccessLayer.FlashEvents;
            }
        }


        public static async Task CalcFlashEvent()
        {
            await GetEvents();
            var flashPairList = new List<FlashPairModel>();
            var startFlash = _events.Where(x => x.EventParam is 7).OrderBy(x => x.Timestamp).ToList();
            var endFlash = _events.Where(x => x.EventParam is 2).OrderBy(x => x.Timestamp).ToList();


            //TODO: Add event handling for event codes 1, 3, 5, 6, 8
            //var autoFlash = _events.Where(x => x.EventParam is 3).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            //var localManualFlash = _events.Where(x => x.EventParam is 4).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            //var faultMonitor = _events.Where(x => x.EventParam is 5).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            //var mmuFlash = _events.Where(x => x.EventParam is 6).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            //var preemptFlash = _events.Where(x => x.EventParam is 8).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            //var otherFlash = _events.Where(x => x.EventParam is 1).OrderBy(x => x.Timestamp).GroupBy(x => x.SignalID).ToList();
            //if (endFlash == null)
            //{
            //    //TODO: Handle this condition
            //}
            foreach (var startSignal in startFlash)
            {
                var signalId = startSignal.SignalID;
                var timestamp = startSignal.Timestamp;
                
                var endSignal = endFlash.FirstOrDefault(x => x.SignalID == signalId && x.Timestamp > timestamp);
                var isOpen = endSignal == null;
                var flashPair = new FlashPairModel(startSignal, endSignal!, signalId, startSignal.EventParam, isOpen);

                endFlash.Remove(endSignal!);
                flashPairList.Add(flashPair);
                Console.WriteLine(flashPair.ToString());
            }
            await FlashEventDataAccessLayer.WriteFlashPairsToDb(flashPairList);

        }

    }
}