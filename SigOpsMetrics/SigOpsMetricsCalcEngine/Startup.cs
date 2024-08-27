using SigOpsMetricsCalcEngine.Calcs;
using SigOpsMetricsCalcEngine.DataAccess;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine
{
    public class Startup
    {
        private static bool RunPreempt;
        private static bool RunFlash;
        private static bool RunCycle;
        private static bool RunRamp;
        private static bool RunPhase;
        //private static readonly string DemoSqlTable = ConfigurationManager.AppSettings["PREEMPT_TABLE_NAME"] ?? "preempt_log";
        private static readonly int MaxDays = int.Parse(ConfigurationManager.AppSettings["MAX_DAYS"] ?? "5");
        //private static readonly int NumWednesday = int.Parse(ConfigurationManager.AppSettings["NUM_WED"] ?? "1");
        internal static string metroCentral = @"SigOpsMC.csv";


        static List<List<long?>> GetSignalList(string filePath)
        { 
            var assembly = Assembly.GetExecutingAssembly();
            var result = new List<List<long?>>();
            using var stream = assembly.GetManifestResourceStream(filePath);
            using var reader = new StreamReader(metroCentral);
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

            return result;
        }

        private static DateTime ConsoleInput(string dateType)
        {
            Console.Write($"Enter a {dateType} date (MM/DD/YYYY): ");
            var start = Console.ReadLine();
            DateTime parsedDate;
            var isValidDate = DateTime.TryParse(start, out parsedDate) && parsedDate < DateTime.Today;
            while (!isValidDate)
            {
                Console.WriteLine("Please try again with a valid date");
                start = Console.ReadLine();
                isValidDate = DateTime.TryParse(start, out parsedDate);
            }
            return parsedDate;
        }
        static bool ContainsNumber(List<List<long?>> doubleIndexedArray, long? number)
        {
            // Flatten the double-indexed array and check if it contains the number
            return doubleIndexedArray.Any(innerList => innerList.Contains(number));
        }

        public static async Task Main(string[] args)
        {

            Console.WriteLine("Welcome to SigOpsTools Calculation Engine v 0.1!");
            Console.WriteLine("This tool will currently calculate the following metrics for you:");
            Console.WriteLine("Cycle Time");
            Console.WriteLine("Phase Detection");


            //TODO: Change all if statements to correspond with console statements
            var startDate = ConsoleInput("start");
            var endDate = ConsoleInput("end");
            var regionCodes = new List<long?>();
            var b = new BaseDataAccessLayer();

            var validDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
             .Select(offset => startDate.AddDays(offset)).ToList();

            Console.WriteLine("Which Region would you like to run calculations on?");
            Console.WriteLine("1. Metro Central");
            Console.WriteLine("2.All");
            var region = Console.ReadLine();
            //Which region would you like to run this for?
            switch (region)
            {
                case "1":
                    regionCodes = GetSignalList(metroCentral).SelectMany(innerList => innerList).ToList();
                    Console.WriteLine("Configuring to Metro Central...");
                    break;
                case "2":
                    Console.WriteLine("Configuring to All signals...");
                    break;
                default:
                    Console.WriteLine("Invalid input. Please try again.");
                    break;

            }
                
            //TODO As other parts of the code are updated, add the other options here
            //TODO The first option added will be incident data between dates for a region
            //Which type of calculation would you like to run?
            Console.WriteLine("Which type of calculation would you like to run?");
            Console.WriteLine("1. Cycle Time");
            Console.WriteLine("2. Phase Detection");
            //Console.WriteLine("3. Preempt");
            //Console.WriteLine("4. Flash");
            //Console.WriteLine("5. Ramp Meter");
            //Console.WriteLine("6. Incident");
            //Console.WriteLine("7. All");

            
            var calcType = Console.ReadLine();
            switch (calcType)
            {
                case "1":
                    RunCycle = true;
                    break;
                case "2":
                    RunPhase = true;
                    break;
                default:
                    Console.WriteLine("Invalid input. Please try again.");
                    break;
            }
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