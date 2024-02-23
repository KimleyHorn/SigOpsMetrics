using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class FlashEventCalc
    {
        /// <summary>
        /// Run the flash event calculation
        /// </summary>
        /// <param name="startDate">The first date to grab signals from</param>
        /// <param name="endDate">The last date to grab signals from</param>
        /// <param name="b"></param>
        /// <returns>True if all operations succeed</returns>
        public static async Task<bool> RunFlash(List<DateTime> validDates, List<BaseEventLogModel> b)
        {

            /* Pseudocode (Process 1) for Flash Events
             * Process 1 takes care of the flash events in the case that there are gaps in the data. Can also be used for normal processing
             * Filter()
             *  Step 1: Check to see which days in the date range have data
             *  CheckDB()
             *  Step 2: Compare the dates in the date range to the dates in the database
             *  FillData()
             *
             * ProcessEvents()
             *  Step 3: Pull the data from S3 for the dates that are not in the database
             *  ProcessEvents()
             *  Step 4: Write Flash events to the database
             */
            var flashFilter = new FlashEventDataAccessLayer(b);
            var isFiltered = await flashFilter.Filter(validDates.FirstOrDefault(), validDates.LastOrDefault());

            if (isFiltered.Count > 0)
            {
                return await flashFilter.Process(isFiltered);
            }

            return true;
        }

    }
}
