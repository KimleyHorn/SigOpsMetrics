using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class RampMeterCalc
    {
        public RampMeterCalc() { }

        public static async Task<bool> RunRamp(List<DateTime> validDates, List<BaseEventLogModel> sigModels)
        {
            var rampFilter = new RampMeterDataAccessLayer(sigModels);
            //var isFiltered = await rampFilter.Filter(validDates.FirstOrDefault(), validDates.LastOrDefault());
            //if (isFiltered.Count > 0)
                return await rampFilter.Process();
            //return false;
        }
    }
}
