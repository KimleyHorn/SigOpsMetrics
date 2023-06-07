using System.Configuration;
using SigOpsMetricsCalcEngine.Calcs;
using SigOpsMetricsCalcEngine.DataAccess;

namespace SigOpsMetricsCalcEngine
{
    public class Startup
    {
        private static readonly bool USE_START_END_DATES = bool.Parse(ConfigurationManager.AppSettings["USE_START_END_DATES"] ?? "false");

        public static async Task Main(string[] args)
        {
            //TODO: Make defaults for date range the previous day
            //If args are specified then use range
            var today = DateTime.Today;
            var dateList = new List<DateTime>();
            var startDate = today.AddDays(-1);
            var testDate = today.AddDays(-5);
            var testEndDate = today.AddDays(-2);

            if (USE_START_END_DATES)
            {
                startDate = DateTime.Parse(ConfigurationManager.AppSettings["START_DATE"] ?? "0");
                var endDate = DateTime.Parse(ConfigurationManager.AppSettings["END_DATE"] ?? "0");
                dateList = new List<DateTime>
                {
                    startDate,
                    endDate
                };
            }
            else
            {
                dateList.Add(startDate);
            }
            await FlashEventCalc.ProcessFlashEvents(startDate);
            //await FlashEventCalc.ProcessFlashEvents(testDate, testEndDate);
        }
    }
}