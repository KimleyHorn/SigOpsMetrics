using SigOpsMetricsCalcEngine.Calcs;
using SigOpsMetricsCalcEngine.DataAccess;
using Microsoft.Extensions.DependencyInjection;


namespace SigOpsMetricsCalcEngine.Startup
{
    public class Startup
    {
        private readonly StartupOps _startupOps = new StartupOps();
        private IServiceCollection _services;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IErrorLogger, ErrorLogger>();
        }


        public static async Task Main(string[] args)
        {
            if (StartupOps.useConsole != StartupOps.customStartEnd)
                throw new ArgumentException("Custom start and end must be enabled to use the console");

            var regionCodes = new List<long?>();

            //Keep this commented until phase information data pulling is added
            //var phaseInformation = new List<string>();
            var validDates = Enumerable.Range(0, (StartupOps.endDate - StartupOps.startDate).Days + 1)
    .Select(offset => StartupOps.startDate.AddDays(offset)).ToList();
            if (StartupOps.startDate == StartupOps.endDate)
            {
                StartupOps.endDate = new DateTime(StartupOps.startDate.Year, StartupOps.startDate.Month, StartupOps.startDate.Day, 23, 59, 59, 999);
                validDates.Add(StartupOps.endDate);
            }
            if (StartupOps.useConsole)
            {
                //Initiate the console application
                StartupOps.Welcome();
                //Gather start and end date
                StartupOps.startDate = StartupOps.ConsoleDate("start");
                StartupOps.endDate = StartupOps.ConsoleDate("end");

                //Determine region
                StartupOps.RegionPicker(regionCodes);
                StartupOps.ConsoleCalc();
                var dirName = string.Empty;
                StartupOps.ConsoleDir(dirName);

#if DEBUG
                StartupOps.ifDebug(StartupOps.startDate, StartupOps.endDate, regionCodes);
#endif
            }
            else if (!StartupOps.customStartEnd)
            {
                StartupOps.noConsoleDir("Archive");
                StartupOps.noConsoleCalc();
#if DEBUG
                StartupOps.ifDebug(StartupOps.startDate, StartupOps.endDate, regionCodes);
#endif
            }

            var b = new BaseDataAccessLayer();

            var archiveDates = new List<DateTime>();
            if (StartupOps.chooseCalc[0])
                archiveDates = await b.GetEventLogsAsync(validDates, "flash_event_log");
            if (StartupOps.chooseCalc[2])
                archiveDates = await b.GetEventLogsAsync(validDates, "preempt_log");

            //Only using sql 
            if (!archiveDates.Any())
            {
                foreach (var t in validDates)
                {
                    Console.WriteLine("Data found in SQL. Processing: " + t.Date);
                }
                if (StartupOps.chooseCalc[0])
                    await FlashEventCalc.Run(validDates, b.SignalEvents, StartupOps.newDirectoryPath);
                if (StartupOps.chooseCalc[2])
                    await PreemptEventCalc.Run(validDates, b.SignalEvents, StartupOps.newDirectoryPath, true);
            }
            //A mix of both
            else
            {
                for (var i = 0; i < archiveDates.Count; i += StartupOps.MaxDays)
                {
                    var currentDate = archiveDates[i];
                    var remainingDays = archiveDates.Count - (i + 1);
                    await b.ProcessEvents(currentDate, signalIdList: regionCodes, StartupOps.eventCodes);
                    if (StartupOps.chooseCalc[0])
                        await FlashEventCalc.Run(archiveDates, b.SignalEvents, StartupOps.newDirectoryPath);
                    if (StartupOps.chooseCalc[2])
                        await PreemptEventCalc.Run(archiveDates, b.SignalEvents, StartupOps.newDirectoryPath, false);
                    b.SignalEvents = [];
                    Console.WriteLine($"There are {remainingDays} left to pull ");
                }
            }
        }
    }
}