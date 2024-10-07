using SigOpsMetricsCalcEngine.Calcs;
using SigOpsMetricsCalcEngine.DataAccess;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using SigOpsMetricsCalcEngine.Models;
using Microsoft.Win32;
using System.Windows;


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
        internal static string newDirectoryPath;
        internal static List<long?>? eventCodes;


        private static List<List<long?>> GetSignalList(string filePath)
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
                    if (long.TryParse(value, out var longValue))
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

        private static DateTime ConsoleDate(string dateType)
        {
            var isValidDate = false;

            do
            {
                try
                {
                    Console.Write($"Enter a {dateType} date (MM/DD/YYYY or M/D/YY): ");
                    var start = Console.ReadLine();
                    isValidDate = DateTime.TryParse(start, out var parsedDate) && parsedDate < DateTime.Today;
                    return parsedDate;
                }
                catch (Exception)
                {
                    Console.WriteLine("Please try again with a valid date");
                }
            } while (!isValidDate);

            throw new InvalidOperationException();
        }

        private static void ConsoleRegion(string? region, List<long?> regionCodes)
        {
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
        }

        private static bool ContainsNumber(List<List<long?>> doubleIndexedArray, long? number)
        {
            // Flatten the double-indexed array and check if it contains the number
            return doubleIndexedArray.Any(innerList => innerList.Contains(number));
        }

        public static async Task Main(string[] args)
        {
            //TODO Fix timeout error and improve memory allocation
            var phaseInformation = new List<string>();

            //Initiate the console application
            Console.WriteLine("Welcome to SigOpsTools Calculation Engine v 0.1!");
            Console.WriteLine("This tool will currently calculate the following metrics for you:");
            Console.WriteLine("Cycle Time");
            Console.WriteLine("Phase Detection");


            //TODO: Change all if statements to correspond with console statements
            //Gather start and end date
            var startDate = ConsoleDate("start");
            var endDate = ConsoleDate("end");
            var regionCodes = new List<long?>();

            var validDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset)).ToList();


            //Determine region
            Console.WriteLine("Which Region would you like to run calculations on?");
            Console.WriteLine("1. Metro Central");
            Console.WriteLine("2.All");
            var region = Console.ReadLine();
            ConsoleRegion(region, regionCodes);

            //TODO As other parts of the code are updated, add the other options here
            //TODO The first option added will be incident data between dates for a region
            //Which type of calculation would you like to run?
            Console.WriteLine("Which type of calculation would you like to run?");
            Console.WriteLine("1. Cycle Time");
            Console.WriteLine("2. Phase Detection");
            Console.WriteLine("3. Preempt");
            //Console.WriteLine("4. Flash");
            //Console.WriteLine("5. Ramp Meter");
            //Console.WriteLine("6. Incident");
            //Console.WriteLine("7. All");


            var calcType = Console.ReadLine();
            eventCodes = new List<long?>();
            switch (calcType)
            {
                //TODO Implement multiple selection
                case "1":
                    RunCycle = true;
                    eventCodes.AddRange([131, 132]);
                    break;
                case "2":
                    RunPhase = true;
                    eventCodes.AddRange([46]);
                    break;
                case "3":
                    RunPreempt = true;
                    eventCodes.AddRange([102, 105, 106, 104, 107, 111, 707, 708]);
                    break;
                default:
                    Console.WriteLine("Invalid input. Please try again.");
                    break;
            }

            string dirName;
            do
            {
                Console.WriteLine("Where would you like this to be written to?");
                Console.WriteLine("Enter Directory Name");
                dirName = Console.ReadLine();

                if (string.IsNullOrEmpty(dirName))
                {
                    throw new NullReferenceException(
                        "Invalid input. Directory name cannot be empty. Please try again.");
                }

                var currentDirectory = Directory.GetCurrentDirectory();
                var parentDirectory = Directory.GetParent(currentDirectory);

                // Combine the parent directory path with the new directory name
                newDirectoryPath = Path.Combine(parentDirectory.FullName, dirName);

                try
                {
                    // Check if the directory already exists
                    if (!Directory.Exists(newDirectoryPath))
                    {
                        // Create the new directory
                        Directory.CreateDirectory(newDirectoryPath);
                        Console.WriteLine("Directory created at: " + newDirectoryPath);
                    }
                    else
                    {
                        Console.WriteLine($"Using existing directory at {newDirectoryPath}");
                    }

                    // Exit the loop since we have a valid directory
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                    Console.WriteLine("Please try again.");
                }
            } while (true);
            var b = new BaseDataAccessLayer();


#if DEBUG
            //Log all of the variables assigned above
            //TODO Figure out if I can add the run booleans to an array for multiple selection ex. [RunCycle, RunPhase, RunPreempt, RunFlash, RunRamp] [0,1,0,0,1]
            Console.WriteLine($"Start Date:{startDate}");
            Console.WriteLine($"End Date:{endDate}");
            if (regionCodes.Count > 0)
                Console.WriteLine($"Region Codes:{regionCodes.Slice(1, 5)}");
            if (RunCycle)
                Console.WriteLine("Running Cycle Time");
            else if (RunPhase)
                Console.WriteLine("Running Phase Detection");
            Console.WriteLine($"Saving files at:{newDirectoryPath}");
            Console.WriteLine("If these are the values you expected press any key...");
            Console.ReadKey(true);

#endif
            validDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset)).ToList();


            if (RunPhase)
            {
                //Creates list of dates from whatever startDate is set to through the previous 7 days
                var dateList = CreateDateList(startDate, endDate);
                validDates.AddRange(dateList);
                regionCodes = GetSignalList(metroCentral).SelectMany(innerList => innerList).ToList();
            }

            if (RunCycle)
            {
                regionCodes = GetSignalList(metroCentral).SelectMany(innerList => innerList).ToList();
            }

            if (RunRamp)
            {
                validDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                    .Select(offset => startDate.AddDays(offset)).ToList();
            }


            

            for (var i = 0; i < validDates.Count; i += MaxDays)
            {
                var currentDate = validDates[i];
                var remainingDays = validDates.Count - i;
                await b.ProcessEvents(currentDate, signalIdList: regionCodes, eventCodes);
                if (RunFlash)
                    await FlashEventCalc.RunFlash(validDates, b.SignalEvents);
                if (RunPreempt)
                    await PreemptEventCalc.RunPreempt(validDates, b.SignalEvents, newDirectoryPath);
                if (RunCycle)
                    await CycleTimeCalc.RunCycle(validDates, b.SignalEvents, newDirectoryPath);
                if (RunPhase)
                    phaseInformation.AddRange(
                        await PhaseDetectionCalc.RunPhase(validDates, b.SignalEvents, regionCodes));
                //await b.ProcessEvents(truncatedDates, signalIdList: regionCodes, eventCodes);
                //    if (RunFlash)
                //        await FlashEventCalc.RunFlash(truncatedDates, b.SignalEvents);
                //    if (RunPreempt)
                //        await PreemptEventCalc.RunPreempt(truncatedDates, b.SignalEvents, newDirectoryPath);
                //    if (RunCycle)
                //        await CycleTimeCalc.RunCycle(truncatedDates, b.SignalEvents, newDirectoryPath);
                //    if (RunRamp)
                //        await RampMeterCalc.RunRamp(truncatedDates, b.SignalEvents);
                //    if (RunPhase)
                //        //configure some way how returning data w/o mem issues 
                //        //Pass 
                //        phaseInformation.AddRange(await PhaseDetectionCalc.RunPhase(truncatedDates, b.SignalEvents, regionCodes));


                b.SignalEvents = [];
                Console.WriteLine($"There are {remainingDays} left to process");
            }

            if (validDates.Count == 0)
            {
                Console.WriteLine("No valid dates found");
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