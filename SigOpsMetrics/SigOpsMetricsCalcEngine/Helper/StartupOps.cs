using System.Configuration;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SigOpsMetricsCalcEngine.Core.Helper;

public static class StartupOps
{
    //ChooseCalc order: [_runCycle (0), _runPhase (1), _runPreempt (2),  _runFlash (3)]
    //TODO implement for phase detection for Dev 2
    //internal static bool useSql;
    internal static bool _runPreempt;
    internal static bool _runFlash;
    internal static bool _runCycle;
    internal static bool _runRamp;
    internal static bool _runPhase;

    internal static bool[] chooseCalc;

    internal static readonly int MaxDays;
    internal static readonly int NumWednesday;
    internal static readonly bool useConsole;
    internal static bool customStartEnd, backFill;
    internal static string metroCentral = @"SigOpsMC.csv";
    internal static string directoryPath;
    internal static List<long?>? eventCodes;
    internal static DateTime startDate;
    internal static DateTime endDate;

    static StartupOps()
    {
        // Initialize boolean flags
        _runCycle = false;
        _runPhase = false;
        _runPreempt = false;
        _runFlash = false;
        _runRamp = false;

        chooseCalc = new bool[] { _runCycle, _runPhase, _runPreempt, _runFlash };

        // Initialize configuration-dependent fields with error handling
        MaxDays = int.TryParse(ConfigurationManager.AppSettings["MAX_DAYS"], out int maxDays) ? maxDays : 5;
        NumWednesday = int.TryParse(ConfigurationManager.AppSettings["NUM_WED"], out int numWed) ? numWed : 1;
        useConsole = bool.TryParse(ConfigurationManager.AppSettings["USE_CONSOLE"], out var useCon) && useCon;
        customStartEnd = bool.TryParse(ConfigurationManager.AppSettings["USE_START_END"], out bool customSE) ? customSE : false;
        backFill = bool.TryParse(ConfigurationManager.AppSettings["BACKFILL"], out bool fillData) ? fillData : false;
        directoryPath = NoConsoleDir("test");
        // Handle startDate and endDate with defaults or error handling
        var startDateString = ConfigurationManager.AppSettings["START_DATE"];
        var endDateString = ConfigurationManager.AppSettings["END_DATE"];

        startDate = DateTime.TryParse(startDateString, out DateTime parsedStartDate) ? parsedStartDate :
            DateTime.Now.Date; // Default to today's date

        endDate = DateTime.TryParse(endDateString, out DateTime parsedEndDate) ? parsedEndDate :
            startDate.AddDays(1); // Default to one day after startDate
    }


    internal static void Welcome()
    {
        Console.WriteLine("Welcome to SigOpsTools Calculation Engine v 0.1!");
        Console.WriteLine("This tool will currently calculate the following metrics for you:");
        Console.WriteLine("Preemption Events");
        Console.WriteLine("Flash Events");
        Console.WriteLine("Phase Detection");

    }

    internal static DateTime ConsoleDate(string dateType)
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

    internal static void RegionPicker(List<long?> regionCodes)
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

    internal static void ConsoleRegion(string? region, List<long?> regionCodes)
    {
        switch (region)
        {
            case "1":
                regionCodes = Enumerable.SelectMany<List<long?>, long?>(GetSignalList(metroCentral), innerList => innerList).ToList();
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

    internal static void ConsoleCalc()
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

    public static string ConsoleDir()
    {
        bool goodDir;
        do
        {
            Console.WriteLine("Where would you like this to be written to?");
            Console.WriteLine("Enter Directory Name");
            var dirName = Console.ReadLine();

            if (string.IsNullOrEmpty(dirName))
            {
                throw new NullReferenceException(
                    "Invalid input. Directory name cannot be empty. Please try again.");
            }

            directoryPath = NoConsoleDir(dirName);
            goodDir = !string.IsNullOrEmpty(directoryPath); 
        } while (!goodDir);

        return directoryPath;
    }
    public static string NoConsoleDir(string dir)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var parentDirectory = Directory.GetParent(currentDirectory);
        var formattedEndDate = endDate.ToString("MM-dd-yyyy");

        // Combine path components with formatted dates
        var newDir = Path.Combine(parentDirectory.FullName, formattedEndDate + dir);
        
        try
        {
            // Check if the directory already exists
            if (!Directory.Exists(newDir))
            {
                // Create the new directory
                Directory.CreateDirectory(newDir);
                Console.WriteLine("Directory created at: " + newDir);
                directoryPath = newDir;
                return newDir;
            }
            else
            {
                Console.WriteLine($"Using existing directory at {newDir}");
                return newDir;
            }

            // Exit the loop since we have a valid directory

        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            Console.WriteLine("Please try again.");
        }

        return newDir;
    }

    internal static List<List<long?>> GetSignalList(string filePath)
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

    internal static bool ContainsNumber(List<List<long?>> doubleIndexedArray, long? number)
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
        else if(_runPreempt)
            Console.WriteLine("Running Preemption");
        Console.WriteLine($"Saving files at:{directoryPath}");
        Console.WriteLine("If these are the values you expected press any key...");
        Console.ReadKey(true);
    }



    internal static void noConsoleCalc()
    {
        //ChooseCalc order: [_runCycle (0), _runPhase (1), _runPreempt (2),  _runFlash (3)]
        _runCycle = bool.Parse(ConfigurationManager.AppSettings["RUN_CYCLE"] ?? "false");
        _runPhase = bool.Parse(ConfigurationManager.AppSettings["RUN_PHASE"] ?? "false");
        _runPreempt = bool.Parse(ConfigurationManager.AppSettings["RUN_PREEMPT"] ?? "false");
        _runFlash = bool.Parse(ConfigurationManager.AppSettings["RUN_FLASH"] ?? "false");
        eventCodes = new List<long?>();
        chooseCalc = [_runFlash, _runCycle, _runPreempt, _runPhase];
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
        }
        catch (Exception e)
        {
            Console.WriteLine("Invalid input. Please try again.");
            Console.WriteLine(e.ToString());
        }
    }


}