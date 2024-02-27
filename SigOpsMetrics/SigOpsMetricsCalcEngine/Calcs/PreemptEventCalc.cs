using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class PreemptEventCalc
    {
        /// <summary>
        /// The method that drives the preempt event processing
        /// </summary>
        /// <param name="validDates">A list of dates in which to process preempt events</param>
        /// <param name="sigModels">A list of signals from the BaseDataAccessLayer</param>
        /// <returns>True if all processes succeed</returns>
        public static async Task<bool> RunPreempt(List<DateTime> validDates, List<BaseEventLogModel> sigModels)
        {
            var preemptFilter = new PreemptEventDataAccessLayer(sigModels);
            var isFiltered = await preemptFilter.Filter(validDates.FirstOrDefault(), validDates.LastOrDefault());

            if (isFiltered.Count > 0)
                return await preemptFilter.Process(isFiltered);
            return true;
        }
    }
}