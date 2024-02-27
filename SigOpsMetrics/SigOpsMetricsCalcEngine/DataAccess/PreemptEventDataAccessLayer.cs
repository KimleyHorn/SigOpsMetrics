using SigOpsMetricsCalcEngine.Models;
using System.Configuration;
using System.Data;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    public class PreemptEventDataAccessLayer : BaseDataAccessLayer, IDataAccess
    {
        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["PREEMPT_TABLE_NAME"];
        private static readonly string? MySqlPreemptTableName = ConfigurationManager.AppSettings["PREEMPT_EVENT_TABLE_NAME"];
        internal static readonly List<long?> EventList = [102, 105, 106, 104, 107, 111, 707, 708];
        private static List<PreemptModel> _preemptList = [];

        /// <summary>
        /// Constructor for PreemptEventDataAccessLayer class that takes a list of BaseEventLogModel
        /// </summary>
        /// <param name="sigModels">A list of BaseEventLogModels that is passed from the BaseDataAccessLayer</param>
        public PreemptEventDataAccessLayer(List<BaseEventLogModel> sigModels)
        {
            SignalEvents = sigModels;
        }

        /// <summary>
        /// A helper method that filters a list of BaseEventLogModels by event code so they can be processed into PreemptModels
        /// </summary>
        /// <param name="events">The input list of BaseEventLogModels</param>
        /// <param name="eventCode">The event code to filter by</param>
        /// <returns>A filtered list of BaseEventLogModels</returns>
        private static List<BaseEventLogModel> FilterByEventCode(List<BaseEventLogModel> events, long eventCode)
        {
            return events.Where(x => x.EventCode == eventCode).OrderBy(x => x.Timestamp).ToList();
        }

        /// <summary>
        /// Converts a list of BaseEventLogModels into a list of PreemptModels
        /// </summary>
        /// <param name="baseSignal">A list of BaseEventLogModels to be filtered and converted to a preempt</param>
        /// <returns>True if the operation succeeds, false otherwise</returns>
        public static async Task<bool> CalcPreemptEvent(List<BaseEventLogModel> baseSignal)
        {
            var inputOn = FilterByEventCode(baseSignal, 102);
            var entryStart = FilterByEventCode(baseSignal, 105);
            var trackClear = FilterByEventCode(baseSignal, 106);
            var externalCallOn = FilterByEventCode(baseSignal, 707);
            var externalCallOff = FilterByEventCode(baseSignal, 708);
            var inputOff = FilterByEventCode(baseSignal, 104);
            var dwellService = FilterByEventCode(baseSignal, 107);
            var exitCall = FilterByEventCode(baseSignal, 111);
            foreach (var signal in inputOn)
            {
                var signalId = signal.SignalID;

                try
                {
                    //Grabs EventParam from startFlashEvent instead of first from group
                    var preemptType = signal.EventParam switch
                    {
                        3 => "EVP",
                        7 => "Flush Preempt",
                        1 => "Railroad",
                        _ => "Default"
                    };
                    var externalOn = false;
                    var externalOff = false;

                    var entryStartEvent =
                        entryStart.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);

                    var externalCallOnEvent =
                        externalCallOn.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);
                    var externalCallOffEvent =
                        externalCallOff.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);
                    var trackClearEvent = trackClear.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);
                    var inputOffEvent = inputOff.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);
                    var dwellServiceEvent = dwellService.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);
                    var exitCallEvent = exitCall.FirstOrDefault(x => x.Timestamp >= signal.Timestamp && x.SignalID == signalId);

                    if (externalCallOnEvent != null)
                    {
                        externalOn = true;
                    }

                    if (externalCallOffEvent != null)
                    {
                        externalOff = true;
                    }

                    var preempt = new PreemptModel(signal.Timestamp, inputOffEvent?.Timestamp,
                        entryStartEvent?.Timestamp, trackClearEvent?.Timestamp, dwellServiceEvent?.Timestamp,
                        exitCallEvent?.Timestamp, signalId, preemptType, externalOff, externalOn);

                    _preemptList.Add(preempt);

                    Console.WriteLine(preempt.ToString());
                }
                catch (Exception e)
                {
                    await WriteToErrorLog("PreemptEventCalc", "CalcPreemptEvents", e);
                    return false;
                }
            }

            return true;
        }

        #region Write to MySQL

        /// <summary>
        /// A method that writes all the filtered BaseEventLogModels to the MySQL database
        /// </summary>
        /// <param name="preempts">An enumerable of BaseEventLogModels that will be written to the MySQL database</param>
        /// <returns>True if the operation is successful, false otherwise</returns>
        /// <exception cref="InvalidOperationException">An exception thrown when the MySQL writer fails</exception>
        public static async Task<bool> WritePreemptSignalsToDb(IEnumerable<BaseEventLogModel> preempts)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Timestamp", typeof(DateTime));
            dataTable.Columns.Add("signalID", typeof(long));
            dataTable.Columns.Add("EventCode", typeof(long));
            dataTable.Columns.Add("EventParam", typeof(long));

            // Populate the DataTable with events data
            foreach (var eventData in preempts)
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
                return await MySqlWriter(MySqlTableName ?? throw new InvalidOperationException(), dataTable);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                throw;
            }
        }

        /// <summary>
        /// a method that writes converted preempt events to the preempt event table in the MySQL database
        /// </summary>
        /// <param name="preempts">A list of preempt event models</param>
        /// <returns>True if the operation is successful, false otherwise</returns>
        /// <exception cref="InvalidOperationException">An exception thrown when the MySQL writer fails</exception>
        public static async Task<bool> WritePreemptEventsToDb(IEnumerable<PreemptModel> preempts)
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
            foreach (var eventData in preempts)
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
            // Open a connection to MySQL
            try
            {
                return await MySqlWriter(MySqlPreemptTableName ?? throw new InvalidOperationException(), dataTable);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                var applicationName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
                if (applicationName != null)
                    await WriteToErrorLog(applicationName, "toMySQL", e);
                throw;
            }
        }

        #endregion Write to MySQL

        /// <summary>
        /// A method that filters a list of BaseEventLogModels by valid dates
        /// </summary>
        /// <param name="startDate">The first date the filter looks at</param>
        /// <param name="endDate">The last date the filter looks at</param>
        /// <returns>A filtered list of BaseEventLogModels</returns>
        public async Task<List<BaseEventLogModel>> Filter(DateTime startDate, DateTime endDate)
        {
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                         .Select(offset => startDate.AddDays(offset)).ToList();
            var validData = await FilterData(allDates, EventList, MySqlTableName ?? " ", "Timestamp");
            return validData;
        }

        /// <summary>
        /// The driver method for preempt event processing inherited from the IDataAccess interface
        /// </summary>
        /// <param name="validSignals">A list of signals filtered by date and event code</param>
        /// <returns>True if the operations succeed, false otherwise</returns>
        public async Task<bool> Process(List<BaseEventLogModel> validSignals)
        {
            if (validSignals.Count == 0)
                return true;
            await CalcPreemptEvent(validSignals);
            return await WritePreemptSignalsToDb(validSignals) && await WritePreemptEventsToDb(_preemptList);
        }
    }
}