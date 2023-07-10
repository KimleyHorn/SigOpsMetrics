using System.Configuration;
using SigOpsMetricsCalcEngine.Calcs;
using SigOpsMetricsCalcEngine.DataAccess;

namespace SigOpsMetricsCalcEngine
{
    public class Startup
    {
        private static readonly bool UseStartEndDates = bool.Parse(ConfigurationManager.AppSettings["USE_START_END_DATES"] ?? "false");

        private static readonly bool PreemptOrFlash =
            bool.Parse(ConfigurationManager.AppSettings["PREEMPT_OR_FLASH"] ?? "false");
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

            if(PreemptOrFlash)
                await PreemptEventCalc.RunPreempt(startDate, endDate);
            else
            {
                BaseDataAccessLayer b = new();
                await b.RunFlash(startDate, endDate);
            }
                
            
        }



    }



}