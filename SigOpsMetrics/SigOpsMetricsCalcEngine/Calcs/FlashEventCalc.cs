using System.Collections.Concurrent;
using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class FlashEventCalc
    {
        /// <summary>
        /// The method that drives the flash event processing
        /// </summary>
        /// <param name="validDates">A list of dates in which to process flash events</param>
        /// <param name="sigModels">A list of signals from the BaseDataAccessLayer</param>
        /// <returns>True if all processes succeed</returns>
        public static async Task<bool> Run(List<DateTime> validDates, ConcurrentBag<BaseEventLogModel> sigModels, string dir)
        {
            var flashFilter = new FlashEventDataAccessLayer(sigModels);
            var isFiltered = await flashFilter.Filter(validDates.FirstOrDefault(), validDates.LastOrDefault());

            if (isFiltered.Count > 0)
                return await flashFilter.Process(isFiltered);

            return true;
        }
    }
}