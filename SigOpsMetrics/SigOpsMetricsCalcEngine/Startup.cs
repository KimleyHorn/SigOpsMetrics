using SigOpsMetricsCalcEngine.Calcs;
using SigOpsMetricsCalcEngine.DataAccess;
using System.Configuration;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine
{
    public class Startup
    {
        private static readonly bool UseStartEndDates = bool.Parse(ConfigurationManager.AppSettings["USE_START_END_DATES"] ?? "false");
        private static readonly bool RunPreempt = bool.Parse(ConfigurationManager.AppSettings["RUN_PREEMPT"] ?? "false");
        private static readonly bool RunFlash = bool.Parse(ConfigurationManager.AppSettings["RUN_FLASH"] ?? "false");
        private static readonly string DemoSqlTable = ConfigurationManager.AppSettings["PREEMPT_TABLE_NAME"] ?? "preempt_log";
        private static readonly int MaxDays = int.Parse(ConfigurationManager.AppSettings["MAX_DAYS"] ?? "5");
        private static readonly string SignalCodeValue = ConfigurationManager.AppSettings["SIGNAL_CODES"] ?? "";

        public static async Task Main(string[] args)
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-1);
            var endDate = default(DateTime);
            var signalCodeArray = SignalCodeValue.Split(',');
            var signalCodes = signalCodeArray.Select(long.Parse).Select(dummy => (long?)dummy).ToList();

            if (UseStartEndDates)
            {
                startDate = DateTime.Parse(ConfigurationManager.AppSettings["START_DATE"] ?? "0");
                endDate = DateTime.Parse(ConfigurationManager.AppSettings["END_DATE"] ?? "0");
            }

            var b = new BaseDataAccessLayer();
            var validDates = await b.FillData(startDate, endDate, signalCodes, DemoSqlTable);
            
            if (validDates.Count > MaxDays)
            {
                
                for (var i = 0; i < validDates.Count; i += MaxDays)
                {
                    var remainingDays = validDates.Count - i;
                    var actualDays = Math.Min(MaxDays, remainingDays);
                    var truncatedDates = validDates.GetRange(i, actualDays);
                    await b.ProcessEvents(truncatedDates);
                    if (RunFlash)
                        await FlashEventCalc.RunFlash(truncatedDates, b.SignalEvents);
                    if (RunPreempt)
                        await PreemptEventCalc.RunPreempt(truncatedDates, b.SignalEvents);
                    b.SignalEvents = [];

                }
            }
            else if (validDates.Count == 0)
            {
                Console.WriteLine("No valid dates found");
            }
            else
            {
                await b.ProcessEvents(validDates);
                if (RunFlash)
                    await FlashEventCalc.RunFlash(validDates, b.SignalEvents);
                if (RunPreempt)
                    await PreemptEventCalc.RunPreempt(validDates, b.SignalEvents);
            }


        }
    }
}