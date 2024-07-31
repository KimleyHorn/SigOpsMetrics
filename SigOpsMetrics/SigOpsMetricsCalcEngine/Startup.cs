using SigOpsMetricsCalcEngine.Calcs;
using SigOpsMetricsCalcEngine.DataAccess;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine
{
    public class Startup
    {
        private static readonly bool UseStartEndDates = bool.Parse(ConfigurationManager.AppSettings["USE_START_END_DATES"] ?? "false");
        private static readonly bool RunPreempt = bool.Parse(ConfigurationManager.AppSettings["RUN_PREEMPT"] ?? "false");
        private static readonly bool RunFlash = bool.Parse(ConfigurationManager.AppSettings["RUN_FLASH"] ?? "false");
        private static readonly bool RunCycle = bool.Parse(ConfigurationManager.AppSettings["RUN_CYCLE"] ?? "false");
        private static readonly bool RunRamp = bool.Parse(ConfigurationManager.AppSettings["RUN_RAMP"] ?? "false");
        private static readonly bool RunPhase = bool.Parse(ConfigurationManager.AppSettings["RUN_PHASE"] ?? "false");
        private static readonly string DemoSqlTable = ConfigurationManager.AppSettings["PREEMPT_TABLE_NAME"] ?? "preempt_log";
        private static readonly int MaxDays = int.Parse(ConfigurationManager.AppSettings["MAX_DAYS"] ?? "5");
        private static readonly string SignalCodeValue = ConfigurationManager.AppSettings["SIGNAL_CODES"] ?? "";
        private static readonly int NumWednesday = int.Parse(ConfigurationManager.AppSettings["NUM_WED"] ?? "1");
        internal static string metroCentral = $"C:\\Users\\alex.valentin\\Downloads\\SigOpsMC.csv";


        static List<List<long?>> GetSignalList(string filePath)
        {
            var result = new List<List<long?>>();

            using (var reader = new StreamReader(filePath))
            {
                string line;

                // Skip the first row
                if ((line = reader.ReadLine()) == null)
                    return result; // Return empty if file is empty or only has one row

                // Process the rest of the rows
                while ((line = reader.ReadLine()) != null)
                {
                    var values = line.Split(',');
                    var longValues = new List<long?>();

                    foreach (var value in values)
                    {
                        if (long.TryParse(value, out long longValue))
                        {
                            longValues.Add(longValue);
                        }
                        else
                        {
                            // Handle the error if needed, e.g., log it or skip
                            Console.WriteLine($"Unable to parse '{value}' as a long?.");
                        }
                    }

                    result.Add(longValues);
                }
            }

            return result;
        }

        static bool ContainsNumber(List<List<long?>> doubleIndexedArray, long? number)
        {
            // Flatten the double-indexed array and check if it contains the number
            return doubleIndexedArray.Any(innerList => innerList.Contains(number));
        }

        public static async Task Main(string[] args)
        {

            var today = DateTime.Today;
            var startDate = today.AddDays(-1);
            var endDate = today;
            var signalCodeArray = SignalCodeValue.Split(',');
            var signalCodes = signalCodeArray.Select(long.Parse).Select(x => (long?)x).ToList();
            var regionCodes = new List<long?>();
            var validDates = new List<DateTime>();

            var phaseInformation = new List<string>();

            if (UseStartEndDates)
            {
                startDate = DateTime.Parse(ConfigurationManager.AppSettings["START_DATE"] ?? "0");
                endDate = DateTime.Parse(ConfigurationManager.AppSettings["END_DATE"] ?? "0");

            }

            var b = new BaseDataAccessLayer();

            if (RunCycle || RunRamp)
            {
                validDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                             .Select(offset => startDate.AddDays(offset)).ToList();
            }

            if (RunPhase)
            {
                //Creates list of dates from whatever startDate is set to through the previous 7 days
                endDate = startDate.AddDays(-7);
                var dateList = CreateDateList(startDate, endDate);
                validDates.AddRange(dateList);

                regionCodes = GetSignalList(metroCentral).SelectMany(innerList => innerList).ToList();
            }

            //if (!RunRamp)
            //    await b.FillData(startDate, endDate, signalCodes, DemoSqlTable);

            if (RunCycle)
            {
                regionCodes = GetSignalList(metroCentral).SelectMany(innerList => innerList).ToList();
            }

            if (RunRamp)
            {
                validDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
             .Select(offset => startDate.AddDays(offset)).ToList();
            }

            if (validDates.Count > MaxDays)
            {

                for (var i = 0; i < validDates.Count; i += MaxDays)
                {
                    var remainingDays = validDates.Count - i;
                    var actualDays = Math.Min(MaxDays, remainingDays);
                    var truncatedDates = validDates.GetRange(i, actualDays);
                    //Figure out how to if/only if run cycle is true due to different spot for data in code
                    //if(RunRamp == RunFlash && RunRamp == RunCycle)
                    //    await b.ProcessEvents(truncatedDates);
                    await b.ProcessEvents(truncatedDates, signalIdList: regionCodes);
                    if (RunFlash)
                        await FlashEventCalc.RunFlash(truncatedDates, b.SignalEvents);
                    if (RunPreempt)
                        await PreemptEventCalc.RunPreempt(truncatedDates, b.SignalEvents);
                    if (RunCycle)
                        await CycleTimeCalc.RunCycle(truncatedDates, b.SignalEvents);
                    if (RunRamp)
                        await RampMeterCalc.RunRamp(truncatedDates, b.SignalEvents);
                    if (RunPhase)
                        //configure some way how returning data w/o mem issues 
                        //Pass 
                        phaseInformation.AddRange(await PhaseDetectionCalc.RunPhase(truncatedDates, b.SignalEvents, regionCodes));
                    

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
                if (RunCycle)
                    await CycleTimeCalc.RunCycle(validDates, b.SignalEvents);
                if (RunPhase)
                    phaseInformation.AddRange(await PhaseDetectionCalc.RunPhase(validDates, b.SignalEvents, regionCodes));
            }
        }

        public static List<DateTime> IsItWednesday(DateTime lastDay, int numWed)
        {
            var wednesday = new List<DateTime>();
            var day = lastDay;
            while (wednesday.Count < numWed)
            {
                if (day.DayOfWeek == DayOfWeek.Wednesday)
                {
                    wednesday.Add(day);
                }
                day = day.AddDays(-1);
            }
            return wednesday;
        }

        public static IEnumerable<DateTime> CreateDateList(DateTime startDateTime, DateTime endDateTime)
        {
            while (endDateTime <= startDateTime)
            {
                yield return endDateTime;
                endDateTime = endDateTime.AddDays(1);
            }
        }
    }
}