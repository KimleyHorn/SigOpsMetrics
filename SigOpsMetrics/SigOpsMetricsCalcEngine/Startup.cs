using System.Configuration;
using SigOpsMetricsCalcEngine.Calcs;
using SigOpsMetricsCalcEngine.DataAccess;

namespace SigOpsMetricsCalcEngine
{
    public class Startup
    {
        private static readonly bool UseStartEndDates = bool.Parse(ConfigurationManager.AppSettings["USE_START_END_DATES"] ?? "false");
        private static readonly bool RunPreempt = bool.Parse(ConfigurationManager.AppSettings["RUN_PREEMPT"] ?? "false");
        private static readonly bool RunFlash = bool.Parse(ConfigurationManager.AppSettings["RUN_FLASH"] ?? "false");

        public static async Task Main(string[] args)
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-1);
            var endDate = default(DateTime);

            if (UseStartEndDates)
            {
                startDate = DateTime.Parse(ConfigurationManager.AppSettings["START_DATE"] ?? "0");
                endDate = DateTime.Parse(ConfigurationManager.AppSettings["END_DATE"] ?? "0");
            }

            // TODO: as more calcs are added, lets go to S3 once and get all the data added to BaseDataAccessLayer.SignalEvents, then process them afterwards
            if (RunFlash)
                await FlashEventCalc.RunFlash(startDate, endDate);
            if (RunPreempt)
                await PreemptEventCalc.RunPreempt(startDate, endDate);
            
        }
    }
}