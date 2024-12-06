using System.Collections.Concurrent;
using SigOpsMetricsCalcEngine.Core.DataAccess;
using SigOpsMetricsCalcEngine.Core.Models;

namespace SigOpsMetricsCalcEngine.Core.Calcs
{
    internal class CycleTimeCalc
    {


        public CycleTimeCalc() { }

        public static async Task<bool> RunCycle(List<DateTime> validDates, ConcurrentBag<BaseEventLogModel> sigModels, string filePath)
        {
            var cycleFilter = new CycleTimeDataAccessLayer(sigModels);
            var isFiltered = await cycleFilter.Filter(validDates.FirstOrDefault(), validDates.LastOrDefault());
            if(isFiltered.Count > 0)
                return await cycleFilter.Process(isFiltered);
            return false;
        }
    }
}
