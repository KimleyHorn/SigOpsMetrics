using System.Configuration;
using SigOpsMetricsCalcEngine.DataAccess;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs
{
    internal class PreemptEventCalc
    {
        private static readonly string MySqlTableName = ConfigurationManager.AppSettings["PREEMPT_TABLE_NAME"] ?? "preempt_log";
        private static readonly string MySqlDbName = ConfigurationManager.AppSettings["DB_NAME"] ?? "test";



        public static async Task<bool> RunPreempt(List<DateTime> validDates, List<BaseEventLogModel> b)
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

            var preemptFilter = new PreemptEventDataAccessLayer(b);
            var isFiltered = await preemptFilter.Filter(validDates.FirstOrDefault(), validDates.LastOrDefault());

            if (isFiltered.Count > 0)
            {
                return await preemptFilter.Process(isFiltered);
            }
            return true;
        }


    }
}
