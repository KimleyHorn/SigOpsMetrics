using System.Collections.Concurrent;
using SigOpsMetricsCalcEngine.Core.DataAccess;
using SigOpsMetricsCalcEngine.Core.Models;

namespace SigOpsMetricsCalcEngine.Core.Calcs
{
    internal class RampMeterCalc
    {
        public RampMeterCalc() { }

        public static async Task<bool> RunRamp(List<DateTime> validDates, ConcurrentBag<BaseEventLogModel> sigModels)
        {
            var rampFilter = new RampMeterDataAccessLayer(sigModels);
            //var isFiltered = await rampFilter.Filter(validDates.FirstOrDefault(), validDates.LastOrDefault());
            //if (isFiltered.Count > 0)
                return await rampFilter.Process();
            //return false;
        }
    }
}
