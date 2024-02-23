using System.Configuration;
using System.Data;
using MySqlConnector;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    public class PreemptEventDataAccessLayer : BaseDataAccessLayer, IDataAccess
    {

        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["PREEMPT_TABLE_NAME"];
        private static readonly string? MySqlPreemptTableName = ConfigurationManager.AppSettings["PREEMPT_EVENT_TABLE_NAME"];
        internal static readonly List<long?> EventList = [102, 105, 106, 104, 107, 111, 707, 708];
        private static List<PreemptModel> preemptList = new List<PreemptModel>();

        public PreemptEventDataAccessLayer(List<BaseEventLogModel> b)
        {
            SignalEvents = b;
        }
        private static List<BaseEventLogModel> FilterByEventCode(List<BaseEventLogModel> events, long eventCode)
        {
            return events.Where(x => x.EventCode == eventCode).OrderBy(x => x.Timestamp).ToList();
        }



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

                    preemptList.Add(preempt);

                    Console.WriteLine(preempt.ToString());
                }
                catch (Exception e)
                {
                    await BaseDataAccessLayer.WriteToErrorLog("PreemptEventCalc", "CalcPreemptEvents", e);
                    return false;

                }

            }

            return await PreemptEventDataAccessLayer.WritePreemptEventsToDb(preemptList);
        }

        #region Write to MySQL
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
                //await WriteToErrorLog(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name, "toMySQL", e);
                throw;
            }
        }

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


        #endregion

        public static async Task<List<BaseEventLogModel>> ReadAllFromMySql()
        {
            var events = new List<BaseEventLogModel>();
            try
            {

                if (MySqlConnection.State == ConnectionState.Closed)
                {
                    await MySqlConnection.OpenAsync();
                }

                await using var cmd = new MySqlCommand($"SELECT t.* FROM {MySqlDbName}.{MySqlTableName} t", MySqlConnection);
                await using var reader = await cmd.ExecuteReaderAsync();


                while (await reader.ReadAsync())
                {
                    var preemptEvent = new BaseEventLogModel
                    {
                        Timestamp = reader.GetDateTime("Timestamp"),
                        SignalID = reader.GetInt64("signalID"),
                        EventCode = reader.GetInt64("EventCode"),
                        EventParam = reader.GetInt64("EventParam")
                    };
                    events.Add(preemptEvent);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally { await MySqlConnection.CloseAsync(); }

            return events;
        }


        public async Task<List<BaseEventLogModel>> Filter(DateTime startDate, DateTime endDate)
        {
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                         .Select(offset => startDate.AddDays(offset)).ToList();
            var validData = await FilterData(allDates, EventList, MySqlTableName ?? " ");
            return validData;
        }

        public async Task<bool> Process(List<BaseEventLogModel> validSignals)
        {
            if (validSignals.Count == 0)
                return true;
            await CalcPreemptEvent(validSignals);
            return await WritePreemptSignalsToDb(validSignals) && await WritePreemptEventsToDb(preemptList);
        }
    }
}
