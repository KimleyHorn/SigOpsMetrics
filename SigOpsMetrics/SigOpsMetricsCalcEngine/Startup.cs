using SigOpsMetricsCalcEngine.Calcs;
using SigOpsMetricsCalcEngine.DataAccess;
using System.Configuration;
using System.Reflection;

namespace SigOpsMetricsCalcEngine
{
    public class Startup
    {
        private static bool _runPreempt, _runFlash, _runCycle, _runRamp, _runPhase;
        //ChooseCalc order: [_runCycle (0), _runPhase (1), _runPreempt (2),  _runFlash (3)]
        private static bool[] chooseCalc = [ _runCycle, _runPhase, _runPreempt, _runFlash];
        private static readonly int MaxDays = int.Parse(ConfigurationManager.AppSettings["MAX_DAYS"] ?? "5");
        //TODO implement for phase detection for Dev 2
        private static readonly int NumWednesday = int.Parse(ConfigurationManager.AppSettings["NUM_WED"] ?? "1");
        private static readonly bool useConsole = bool.Parse(ConfigurationManager.AppSettings["USE_CONSOLE"] ?? "false");
        private static bool customStartEnd = bool.Parse(ConfigurationManager.AppSettings["USE_START_END"] ?? "false");
        internal static string metroCentral = @"SigOpsMC.csv";
        internal static string newDirectoryPath;
        internal static List<long?>? eventCodes;
        //internal static bool useSql;
        static DateTime startDate = DateTime.Parse(ConfigurationManager.AppSettings["START_DATE"]);
        static DateTime endDate = DateTime.Parse(ConfigurationManager.AppSettings["END_DATE"]);

        #region Console Log Methods

        private static void Welcome()
        {
            Console.WriteLine("Welcome to SigOpsTools Calculation Engine v 0.1!");
            Console.WriteLine("This tool will currently calculate the following metrics for you:");
            Console.WriteLine("Preemption Events");
            Console.WriteLine("Flash Events");
            Console.WriteLine("Phase Detection");

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

        private static void RegionPicker(List<long?> regionCodes)
        {
            var regionList = new string[] { "Metro Central", "All" };
            Console.WriteLine("Which Region would you like to run calculations on?");

            for (var i = 0; i < regionList.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {regionList[i]}");
            }

            var region = Console.ReadLine()!;

            ConsoleRegion(region, regionCodes);
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
                    RegionPicker(new List<long?>());
                    break;

            }
        }

        private static void ConsoleCalc()
        {
            //ChooseCalc order: [_runCycle (0), _runPhase (1), _runPreempt (2),  _runFlash (3)]

            //Which type of calculation would you like to run?
            Console.WriteLine("Which type of calculation would you like to run?");
            Console.WriteLine("1. Cycle Time");
            Console.WriteLine("2. Phase Detection");
            Console.WriteLine("3. Preempt");
            Console.WriteLine("4. Flash");
            //Console.WriteLine("5. Ramp Meter");
            //Console.WriteLine("6. Incident");
            //Console.WriteLine("7. All");

            //TODO Helper method
            var calcType = Console.ReadLine().Split(',');
            eventCodes = new List<long?>();
            foreach (var t in calcType)
            {
                chooseCalc[int.Parse(t) - 1] = true;
            }
            //TODO Implement multiple selection
            if (calcType.Contains("1"))
            {
                eventCodes.AddRange([131, 132]);
            }

            if (calcType.Contains("2"))
            {
                eventCodes.AddRange([46]);
            }

            if (calcType.Contains("3"))
            {
                eventCodes.AddRange([102, 105, 106, 104, 107, 111, 707, 708]);
            }

            if (calcType.Contains("4"))
            {
                eventCodes.AddRange([173]);
            }
            //TODO Figure out if/else logic 
            //else
            //{
            //    Console.WriteLine("Invalid input. Please try again.");
            //}
        }

        public static void ConsoleDir(string dirName)
        {
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
        }

        #endregion

        #region Helper Methods

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

        private static bool ContainsNumber(List<List<long?>> doubleIndexedArray, long? number)
        {
            // Flatten the double-indexed array and check if it contains the number
            return doubleIndexedArray.Any(innerList => innerList.Contains(number));
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

        public static void ifDebug(DateTime startDate, DateTime endDate, List<long?> regionCodes)
        {
            //Log all of the variables assigned above
            Console.WriteLine($"Start Date:{startDate}");
            Console.WriteLine($"End Date:{endDate}");
            if (regionCodes.Count > 0)
                Console.WriteLine($"Region Codes:{regionCodes.Slice(1, 5)}");
            if (_runCycle)
                Console.WriteLine("Running Cycle Time");
            else if (_runPhase)
                Console.WriteLine("Running Phase Detection");
            Console.WriteLine($"Saving files at:{newDirectoryPath}");
            Console.WriteLine("If these are the values you expected press any key...");
            Console.ReadKey(true);
        }

        public static void noConsoleDir(string dir)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var parentDirectory = Directory.GetParent(currentDirectory);
            var formattedStartDate = startDate.ToString("MM-dd-yyyy");
            var formattedEndDate = endDate.ToString("MM-dd-yyyy");

            // Combine path components with formatted dates
            newDirectoryPath = Path.Combine(parentDirectory.FullName, formattedEndDate + dir);

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
            
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                Console.WriteLine("Please try again.");
            }
        }


        private static void noConsoleCalc()
        {
            //ChooseCalc order: [_runCycle (0), _runPhase (1), _runPreempt (2),  _runFlash (3)]
            _runCycle = bool.Parse(ConfigurationManager.AppSettings["RUN_CYCLE"] ?? "false");
            _runPhase = bool.Parse(ConfigurationManager.AppSettings["RUN_PHASE"] ?? "false");
            _runPreempt = bool.Parse(ConfigurationManager.AppSettings["RUN_PREEMPT"] ?? "false");
            _runFlash = bool.Parse(ConfigurationManager.AppSettings["RUN_FLASH"] ?? "false");
            eventCodes = new List<long?>();
            chooseCalc = [ _runFlash, _runCycle,_runPreempt, _runPhase];
            //TODO Implement multiple selection

            try
            {
                if (chooseCalc[0])
                {
                    eventCodes.AddRange([173]);
                }

                if (chooseCalc[1])
                {
                    eventCodes.AddRange([46]);
                }

                if (chooseCalc[2])
                {
                    eventCodes.AddRange([102, 105, 106, 104, 107, 111, 707, 708]);
                }

                if (chooseCalc[3])
                {
                    eventCodes.AddRange([131, 132]);

                }
            }catch(Exception e)
            {
                Console.WriteLine("Invalid input. Please try again.");
                Console.WriteLine(e.ToString());
            }
        }


        #endregion

        public static async Task Main(string[] args)
        {
            if (useConsole != customStartEnd)
                throw new ArgumentException("Custom start and end must be enabled to use the console");

            var regionCodes = new List<long?>();

            //Keep this commented until phase information data pulling is added
            //var phaseInformation = new List<string>();
            var validDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
    .Select(offset => startDate.AddDays(offset)).ToList();
            if (startDate == endDate)
            {
                endDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, 23, 59, 59, 999);
                validDates.Add(endDate);
            }
            if (useConsole)
            {
                //Initiate the console application
                Welcome();
                //Gather start and end date
                startDate = ConsoleDate("start");
                endDate = ConsoleDate("end");

                //Determine region
                RegionPicker(regionCodes);
                ConsoleCalc();
                var dirName = string.Empty;
                ConsoleDir(dirName);

#if DEBUG
                ifDebug(startDate, endDate, regionCodes);
#endif
            }
            else if (!customStartEnd)
            {
                noConsoleDir("Archive");
                noConsoleCalc();
#if DEBUG
                ifDebug(startDate, endDate, regionCodes);
#endif
            }

            var b = new BaseDataAccessLayer();

            var archiveDates = new List<DateTime>();
            if (chooseCalc[0])
                archiveDates = await b.GetEventLogsAsync(validDates, "flash_event_log");
            if (chooseCalc[2])
                archiveDates = await b.GetEventLogsAsync(validDates, "preempt_log");

            //Only using sql 
            if (!archiveDates.Any())
            {
                foreach (var t in validDates)
                {
                    Console.WriteLine("Data found in SQL. Processing: " + t.Date);
                }
                if (chooseCalc[0])
                    await FlashEventCalc.Run(validDates, b.SignalEvents, newDirectoryPath);
                if (chooseCalc[2])
                    await PreemptEventCalc.Run(validDates, b.SignalEvents, newDirectoryPath, true);
            }
            //A mix of both
            else
            {
                for (var i = 0; i < archiveDates.Count; i += MaxDays)
                {
                    var currentDate = archiveDates[i];
                    var remainingDays = archiveDates.Count - (i + 1);
                    await b.ProcessEvents(currentDate, signalIdList: regionCodes, eventCodes);
                    if (chooseCalc[0])
                        await FlashEventCalc.Run(archiveDates, b.SignalEvents, newDirectoryPath);
                    if (chooseCalc[2])
                        await PreemptEventCalc.Run(archiveDates, b.SignalEvents, newDirectoryPath, false);
                    b.SignalEvents = [];
                    Console.WriteLine($"There are {remainingDays} left to pull ");
                }
            }
        }
    }
}