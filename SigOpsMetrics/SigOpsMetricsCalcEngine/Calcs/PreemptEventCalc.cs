using SigOpsMetricsCalcEngine.Core.DataAccess;

namespace SigOpsMetricsCalcEngine.Core.Calcs
{
    internal class PreemptEventCalc : ICalc
    {
        /// <summary>
        /// The method that drives the preempt event processing
        /// </summary>
        /// <param name="validDates">A list of dates in which to process preempt events</param>
        /// <param name="sigModels">A list of signals from the BaseDataAccessLayer</param>
        /// <param name="dir">Directory where a csv of results is stored</param>
        /// <param name="archiveFlag">Boolean that captures whether or not the archive is used</param>
        /// <returns>True if all processes succeed</returns>
        public static async Task<bool> Run(List<DateTime> validDates, BaseDataAccessLayer sigModels, string dir, bool archiveFlag = false)
        {
            try
            {
                var preemptFilter = new PreemptEventDataAccessLayer(sigModels, validDates, dir);
                var isFiltered = await preemptFilter.Filter(validDates.FirstOrDefault(), validDates.LastOrDefault());
                return await preemptFilter.Process(isFiltered, archiveFlag);
            }
            catch (Exception ex)
            {
                return false;
            }


        }


    }
}