using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class PreemptEventCalc
    {

        //TODO add preempt event parameter
        //TODO throw out any event without an exit call
        private static ConcurrentBag<PreemptModel> _preemptCollection = [];
        private static string dir;

        /// <summary>
        /// The method that drives the preempt event processing
        /// </summary>
        /// <param name="validDates">A list of dates in which to process preempt events</param>
        /// <param name="sigModels">A list of signals from the BaseDataAccessLayer</param>
        /// <returns>True if all processes succeed</returns>
        public static async Task<bool> RunPreempt(List<DateTime> validDates, ConcurrentBag<BaseEventLogModel> sigModels, string dir)
        {
            try
            {
                var preemptFilter = new PreemptEventDataAccessLayer(sigModels, validDates, dir);
                //var isFiltered = await preemptFilter.Filter(validDates.FirstOrDefault(), validDates.LastOrDefault());
                return await preemptFilter.Process(sigModels);
            }
            catch (Exception ex)
            {
                return false;

            }


        }


    }
}