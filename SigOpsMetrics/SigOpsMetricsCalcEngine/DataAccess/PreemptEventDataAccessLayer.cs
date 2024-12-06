using System.Collections.Concurrent;
using System.Configuration;
using System.Data;
using System.Text;
using SigOpsMetricsCalcEngine.Core.Models;

namespace SigOpsMetricsCalcEngine.Core.DataAccess
{
    public class PreemptEventDataAccessLayer : IDataAccess
    {

        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["PREEMPT_TABLE_NAME"];
        private static readonly string? MySqlPreemptTableName = ConfigurationManager.AppSettings["PREEMPT_EVENT_TABLE_NAME"];
        internal static readonly List<long?> EventList = [102, 105, 106, 104, 107, 111, 707, 708];
        private static ConcurrentBag<PreemptModel> _preemptList = [];
        private static ConcurrentBag<PreemptModel> _badList = [];
        private static List<DateTime> validDates = [];
        private static string dir;
        private static readonly object Lock = new object();
        private static BaseDataAccessLayer data;

        /// <summary>
        /// Constructor for PreemptEventDataAccessLayer class that takes a list of BaseEventLogModel
        /// </summary>
        /// <param name="sigModels">A list of BaseEventLogModels that is passed from the BaseDataAccessLayer</param>
        public PreemptEventDataAccessLayer(BaseDataAccessLayer b, List<DateTime> dates, string dirPath)
        {
            data = b;
            validDates = dates;
            dir = dirPath;
        }


        /// <summary>
        /// Converts a list of BaseEventLogModels into a list of PreemptModels
        /// </summary>
        /// <param name="baseSignal">A list of BaseEventLogModels to be filtered and converted to a preempt</param>
        /// <returns>True if the operation succeeds, false otherwise</returns>
        public async Task<bool> Calc(ConcurrentBag<BaseEventLogModel> baseSignal)
        {
            //TODO Add time delta between input on and entry start
            //Definitely use this logic as a go by to see how to grab data list
            var inputOn = await BaseDataAccessLayer.FilterByEventCode(baseSignal, 102);
            var entryStart = await BaseDataAccessLayer.FilterByEventCode(baseSignal, 105);
            var trackClear = await BaseDataAccessLayer.FilterByEventCode(baseSignal, 106);
            var externalCallOn = await BaseDataAccessLayer.FilterByEventCode(baseSignal, 707);
            var externalCallOff = await BaseDataAccessLayer.FilterByEventCode(baseSignal, 708);
            var inputOff = await BaseDataAccessLayer.FilterByEventCode(baseSignal, 104);
            var dwellService = await BaseDataAccessLayer.FilterByEventCode(baseSignal, 107);
            var exitCall = await BaseDataAccessLayer.FilterByEventCode(baseSignal, 111);
            foreach (var signal in inputOn)
            {

                var signalId = signal.SignalID;
                var nextSignal = inputOn.Where(x => x.SignalID == signal.SignalID && x.Timestamp > signal.Timestamp).MinBy(x => x.Timestamp);

                try
                {
                    //Grabs EventParam from startFlashEvent instead of first from group
                    var externalOn = false;
                    var externalOff = false;
                    var entryStartEvent = entryStart
                        .Where(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId).MinBy(y => y.Timestamp);
                    var externalCallOnEvent =
                        externalCallOn.Where(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId)
                            .MinBy(y => y.Timestamp);
                    var externalCallOffEvent =
                        externalCallOff.Where(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId)
                            .MinBy(y => y.Timestamp);
                    var trackClearEvent = trackClear
                        .Where(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId).MinBy(y => y.Timestamp);

                    //Determine whether an event has a track clear parameter based on the events and event codes provided
                    //Humor requested by Senior PM
                    var isChooChoo = trackClearEvent is not null;
                    var preemptType = isChooChoo switch
                    {
                        true => "Railroad",
                        _ => "Other"
                    };

                    var inputOffEvent = inputOff.Where(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId)
                        .MinBy(y => y.Timestamp);
                    var dwellServiceEvent = dwellService
                        .Where(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId).MinBy(y => y.Timestamp);
                    var exitCallEvent = exitCall.Where(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId)
                        .MinBy(y => y.Timestamp);
                    if (exitCallEvent == null || inputOffEvent == null)
                        continue;
                    if (externalCallOnEvent != null)
                    {
                        externalOn = true;
                    }

                    if (externalCallOffEvent != null)
                    {
                        externalOff = true;
                    }

                    var preempt = new PreemptModel(signal.Timestamp, inputOffEvent.Timestamp,
                        entryStartEvent?.Timestamp, trackClearEvent?.Timestamp, dwellServiceEvent?.Timestamp,
                        exitCallEvent?.Timestamp, signalId, preemptType, externalOff, externalOn);

                    //Console.WriteLine("Failed Preempts----------------------------------------");
                    if (nextSignal != null && (preempt.InputOff > nextSignal.Timestamp &&
                                               preempt.ExitCall > nextSignal.Timestamp))
                    {
                        //Console.WriteLine(preempt.ToString());
                        _badList.Add(preempt);
                        continue;
                    }
                    //Console.WriteLine("Failed Preempts----------------------------------------");
                    //Go through _preempt list and find the "last" preempt and compare 
                    _preemptList.Add(preempt);
                    //Console.WriteLine(preempt.ToString());
                }
                catch (Exception ex)
                {
                    await BaseDataAccessLayer._logger.WriteToErrorLogAsync("PreemptEventCalc", "Calc", ex);
                    return false;
                }
            }

            return true;
        }

        #region Write to MySQL

        /// <summary>
        /// A method that writes all the filtered BaseEventLogModels to the MySQL database
        /// </summary>
        /// <param name="signal">An enumerable of BaseEventLogModels that will be written to the MySQL database</param>
        /// <returns>True if the operation is successful, false otherwise</returns>
        /// <exception cref="InvalidOperationException">An exception thrown when the MySQL writer fails</exception>
        public async Task<bool> SignalToDB(ConcurrentBag<BaseEventLogModel> signal)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Timestamp", typeof(DateTime));
            dataTable.Columns.Add("SignalID", typeof(long));
            dataTable.Columns.Add("EventCode", typeof(long));
            dataTable.Columns.Add("EventParam", typeof(long));

            // Populate the DataTable with events data
            foreach (var eventData in signal)
            {
                dataTable.Rows.Add(
                    eventData.Timestamp,
                    eventData.SignalID,
                    eventData.EventCode,
                    eventData.EventParam

                );
            }
            // Open a connection to MySQL
            try
            {
                return await BaseDataAccessLayer.MySqlWriter(MySqlTableName ?? throw new InvalidOperationException(), dataTable);
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer._logger.WriteToErrorLogAsync("PreemptEventDataAccessLayer", "WritePreemptSignalsToDb", ex);
                throw;
            }
        }
        public Task<bool> SignalToCsv(ConcurrentBag<BaseEventLogModel> signal)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// a method that writes converted preempt events to the preempt event table in the MySQL database
        /// </summary>
        /// <param name="events">A list of preempt event models</param>
        /// <returns>True if the operation is successful, false otherwise</returns>
        /// <exception cref="InvalidOperationException">An exception thrown when the MySQL writer fails</exception>
        public async Task<bool> EventToDB(ConcurrentBag<PreemptModel> events)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("InputOn", typeof(DateTime));
            dataTable.Columns.Add("EntryStart", typeof(DateTime));
            dataTable.Columns.Add("TrackClear", typeof(DateTime));
            dataTable.Columns.Add("InputOff", typeof(DateTime));
            dataTable.Columns.Add("DwellService", typeof(DateTime));
            dataTable.Columns.Add("ExitCall", typeof(DateTime));
            dataTable.Columns.Add("SignalID", typeof(long));
            dataTable.Columns.Add("Duration", typeof(long));
            dataTable.Columns.Add("PreemptType", typeof(string));
            dataTable.Columns.Add("ExternalCallOn", typeof(bool));
            dataTable.Columns.Add("ExternalCallOff", typeof(bool));

            // Populate the DataTable with events data
            foreach (var eventData in events)
            {
                dataTable.Rows.Add(
                eventData.InputOn,
                eventData.EntryStart,
                eventData.TrackClear,
                eventData.InputOff,
                eventData.DwellService,
                eventData.ExitCall,
                eventData.SignalID,
                TimeSpan.FromTicks(eventData.Duration.Ticks).TotalSeconds,
                eventData.PreemptType,
                eventData.ExternalCallOn,
                eventData.ExternalCallOff
            );
            }
            
            try
            {
                // Open a connection to MySQL
                return await BaseDataAccessLayer.MySqlWriter(MySqlPreemptTableName ?? throw new InvalidOperationException(), dataTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error" + ex);
                await BaseDataAccessLayer._logger.WriteToErrorLogAsync("PreemptEventDataAccessLayer", "WritePreemptSignalsToDb", ex);
            }

            return false;
        }

        #endregion Write to MySQL

        #region Write to CSV

        public static async Task WritePreemptToCsv(DateTime date, string dirPath, ConcurrentBag<PreemptModel> preemptBag)
        {
            try
            {
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath); // Create the directory if it does not exist
                }

                var csvContent = new StringBuilder();
                //Header Line 
                csvContent.AppendLine("Input On,Entry Start,Track Clear,Input Off,Dwell Service,Exit Call,Signal ID,Duration,Preempt Type,External Call On,External Call Off");

                //Writing Objects to CSV
                foreach (var signalInfo in preemptBag.Where(x => x.InputOn.Value.Date == date.Date))
                {
                    var line = $"{signalInfo.InputOn},{signalInfo.EntryStart},{signalInfo.TrackClear},{signalInfo.InputOff},{signalInfo.DwellService},{signalInfo.ExitCall},{signalInfo.SignalID},{signalInfo.Duration},{signalInfo.PreemptType},{signalInfo.ExternalCallOn},{signalInfo.ExternalCallOff}";
                    csvContent.AppendLine(line);
                }

                var fileDate = date.ToString("yyyy-M-dd");
                var filePath = dirPath + $@"\PreemptDetectionCalc_{fileDate}.csv";
                await File.WriteAllTextAsync(filePath, csvContent.ToString());
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Error: Access to the path is denied. " + ex.Message);
                await BaseDataAccessLayer._logger.WriteToErrorLogAsync("PreemptEventDataAccessLayer", "WritePreemptToCsv", ex);

            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine("Error: Directory not found. " + ex.Message);
                await BaseDataAccessLayer._logger.WriteToErrorLogAsync("PreemptEventDataAccessLayer", "WritePreemptToCsv", ex);

            }
            catch (IOException ex)
            {
                Console.WriteLine("Error: IO exception. " + ex.Message);
                await BaseDataAccessLayer._logger.WriteToErrorLogAsync("PreemptEventDataAccessLayer", "WritePreemptToCsv", ex);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: An unexpected error occurred. " + ex.Message);
                await BaseDataAccessLayer._logger.WriteToErrorLogAsync("PreemptEventDataAccessLayer", "WritePreemptToCsv", ex);

            }
        }

        #endregion

        /// <summary>
        /// A method that filters a list of BaseEventLogModels by valid dates
        /// </summary>
        /// <param name="startDate">The first date the filter looks at</param>
        /// <param name="endDate">The last date the filter looks at</param>
        /// <returns>A filtered list of BaseEventLogModels</returns>
        public async Task<ConcurrentBag<BaseEventLogModel>> Filter(DateTime startDate, DateTime endDate)
        {
            try
            {
                var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                    .Select(offset => startDate.AddDays(offset)).ToList();
                var validData =
                    new ConcurrentBag<BaseEventLogModel>(await data.FilterData(allDates, EventList, MySqlTableName ?? " ",
                        "Timestamp"));
                return validData;
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer._logger.WriteToErrorLogAsync("PreemptEventDataAccessLayer", "WritePreemptToCsv", ex);
                throw;
            }
            
        }

        /// <summary>
        /// The driver method for preempt event processing inherited from the IDataAccess interface
        /// </summary>
        /// <param name="validSignals">A list of signals filtered by date and event code</param>
        /// <param name="archiveFlag"></param>
        /// <returns>True if the operations succeed, false otherwise</returns>
        public async Task<bool> Process(ConcurrentBag<BaseEventLogModel> validSignals, bool archiveFlag)
        {
            try
            {
                await BaseDataAccessLayer.StateSwitcherAsync();
                if (validSignals.Count == 0)
                {
                    await BaseDataAccessLayer._logger.WriteToErrorLogAsync("PreemptEventDataAccessLayer", "Process", new Exception("Preempt filter returned 0 signals. Please try again"));
                    return false;
                }

                await Calc(validSignals);
                var date = validSignals.FirstOrDefault().Timestamp;
                await WritePreemptToCsv(date, dir, _preemptList);
                if (!archiveFlag) return await EventToDB(_preemptList) && await SignalToDB(validSignals);
                Console.WriteLine("Signals already in DB");
                return true;
            }
            catch (Exception ex)
            {
                await BaseDataAccessLayer._logger.WriteToErrorLogAsync("PreemptEventDataAccessLayer", "Process", ex);
                throw;
            }


        }
    }


}