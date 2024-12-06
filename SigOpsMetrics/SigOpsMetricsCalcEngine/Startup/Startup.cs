using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SigOpsMetricsCalcEngine.Core.Calcs;
using SigOpsMetricsCalcEngine.Core.DataAccess;
using SigOpsMetricsCalcEngine.Core.Extensions;


namespace SigOpsMetricsCalcEngine.Core.Startup
{
    public class Startup
    {
        private IServiceCollection _services;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IErrorLogger, ErrorLogger>();
        }


        public static async Task Main(string[] args)
        {

            //Argument interference check
            //If using the console then there will always be a custom start/end date so variables have to match
            if (StartupOps.useConsole != StartupOps.customStartEnd)
                throw new ArgumentException("Custom start and end must be enabled to use the console");

            var regionCodes = new List<long?>();

            //Keep this commented until phase information data pulling is added
            //var phaseInformation = new List<string>();
            var validDates = Enumerable.Range(0, (StartupOps.endDate - StartupOps.startDate).Days + 1)
                .Select(offset => StartupOps.startDate.AddDays(offset)).ToList();


            if (StartupOps.startDate == StartupOps.endDate)
            {
                StartupOps.endDate = new DateTime(StartupOps.startDate.Year, StartupOps.startDate.Month,
                    StartupOps.startDate.Day, 23, 59, 59, 999);
                validDates.Add(StartupOps.endDate);
            }

            if (StartupOps.useConsole)
            {
                //Initiate the console application
                StartupOps.Welcome();
                Console.WriteLine("Do you want to backfill data?");
                var fill = Console.ReadLine();
                if (fill == "T" || fill == "True")
                {
                    StartupOps.backFill = true;
                }
                else if (fill== "F" || fill  == "False")
                {
                    StartupOps.backFill = false;
                }

                //Gather start and end date
                StartupOps.startDate = StartupOps.ConsoleDate("start");
                StartupOps.endDate = StartupOps.ConsoleDate("end");
                //Determine region
                StartupOps.RegionPicker(regionCodes);
                StartupOps.ConsoleCalc();
                if (StartupOps.backFill)
                {
                    if (StartupOps.chooseCalc[0])
                        validDates = await BaseDataAccessLayer.CheckDBAsync("flash_event_log", "Timestamp", "mark1",
                            StartupOps.startDate, StartupOps.endDate);
                    else if (StartupOps.chooseCalc[2])
                        validDates = await BaseDataAccessLayer.CheckDBAsync("preempt_log", "Timestamp", "mark1",
                            StartupOps.startDate, StartupOps.endDate);

                }
                else
                {
                    validDates = Enumerable.Range(0, (StartupOps.endDate - StartupOps.startDate).Days + 1)
                        .Select(offset => StartupOps.startDate.AddDays(offset)).ToList();
                }
                //StartupOps.ConsoleDir();

#if DEBUG
                StartupOps.ifDebug(StartupOps.startDate, StartupOps.endDate, regionCodes);
#endif
            }
            else if (!StartupOps.customStartEnd)
            {
                //StartupOps.NoConsoleDir("Archive");
                StartupOps.noConsoleCalc();
#if DEBUG
                StartupOps.ifDebug(StartupOps.startDate, StartupOps.endDate, regionCodes);
#endif
            }

            var b = new BaseDataAccessLayer();
            var archiveDates = new List<DateTime>();
            if (!StartupOps.backFill)
            {
                if (StartupOps.chooseCalc[0])
                    archiveDates = await b.GetEventLogsAsync(validDates, "flash_event_log");
                if (StartupOps.chooseCalc[2])
                    archiveDates = await b.GetEventLogsAsync(validDates, "preempt_log");
            }


            //Only using sql 
            if (!archiveDates.Any())
            {
                foreach (var t in validDates)
                {
                    Console.WriteLine("Data found in SQL. Processing: " + t.Date);
                }

                if (StartupOps.chooseCalc[0])
                    await FlashEventCalc.Run(validDates, b, StartupOps.ConsoleDir());
                if (StartupOps.chooseCalc[2])
                    await PreemptEventCalc.Run(validDates, b, StartupOps.ConsoleDir(), true);
            }
            //A mix of both
            else
            {
                for (var i = 0; i < archiveDates.Count; i += StartupOps.MaxDays)
                {
                    var currentDate = archiveDates[i];
                    var remainingDays = archiveDates.Count - (i + 1);
                    await b.ProcessEvents(currentDate, signalIdList: regionCodes, StartupOps.eventCodes);
                    await BaseDataAccessLayer.StateSwitcherAsync(true);
                    if (StartupOps.chooseCalc[0])
                        //TODO Reopen SQL connection
                        await FlashEventCalc.Run(archiveDates, b, StartupOps.directoryPath);
                    if (StartupOps.chooseCalc[2])
                        await PreemptEventCalc.Run(archiveDates, b, StartupOps.directoryPath, false);
                    b.SignalEvents = [];
                    Console.WriteLine($"There are {remainingDays} left to pull ");
                }
            }
        }
    }
}