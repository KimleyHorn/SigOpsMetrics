using SigOpsMetricsCalcEngine.Core.DataAccess;

namespace SigOpsMetricsCalcEngine.Core.Calcs
{
    internal class FlashEventCalc : ICalc
    {
        /// <summary>
        /// The method that drives the flash event processing
        /// </summary>
        /// <param name="validDates">A list of dates in which to process flash events</param>
        /// <param name="sigModels">A list of signals from the BaseDataAccessLayer</param>
        /// <returns>True if all processes succeed</returns>
        public static async Task<bool> Run(List<DateTime> validDates, BaseDataAccessLayer sigModels, string dir, bool archiveFlag = false)
        {
            var flashFilter = new FlashEventDataAccessLayer(sigModels);
            var isFiltered = await flashFilter.Filter(validDates.FirstOrDefault(), validDates.LastOrDefault());

            if (isFiltered.Count > 0)
                return await flashFilter.Process(isFiltered, archiveFlag);

            return true;
        }
    }
}