using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class CycleTimeCalc
    {

        public CycleTimeCalc() { }

        public static async Task<bool> RunCycle(List<DateTime> validDates, List<BaseEventLogModel> sigModels)
        {
            var cycleFilter = new CycleTimeDataAccessLayer(sigModels);
            var isFiltered = await cycleFilter.Filter(validDates.FirstOrDefault(), validDates.LastOrDefault());
            if(isFiltered.Count > 0)
                return await cycleFilter.Process(isFiltered);
            return false;
        }
    }
}
