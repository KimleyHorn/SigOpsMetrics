using System.Configuration;
using System.Data;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    public class FlashEventDataAccessLayer : BaseDataAccessLayer, IDataAccess
    {

        
        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["FLASH_EVENT_TABLE_NAME"] ?? "flash_event_log";
        internal static readonly List<long?> EventList = [173];

        public FlashEventDataAccessLayer(List<BaseEventLogModel> b)
        {
            SignalEvents = b;
        }

        #region Write to MySQL
        public static async Task<bool> WriteFlashEventsToDb(IEnumerable<BaseEventLogModel> events)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Timestamp", typeof(DateTime));
            dataTable.Columns.Add("signalID", typeof(long));
            dataTable.Columns.Add("EventCode", typeof(long));
            dataTable.Columns.Add("EventParam", typeof(long));

            // Populate the DataTable with events data
            foreach (var eventData in events)
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
        #endregion





        public async Task<List<BaseEventLogModel>> Filter(DateTime startDate, DateTime endDate)
        {

            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                         .Select(offset => startDate.AddDays(offset)).ToList();
            var validData = await FilterData(allDates, EventList, MySqlTableName ?? " ");
            return validData;
        }


        #region Driver Method
        /// <summary>
        /// Driver method for the flash event processing
        /// </summary>
        /// <param name="validSignals">A list of signals that will be written to the MySQL database</param>
        /// <returns> true if the database is written to or if there are no signals to write to the database</returns>
        public async Task<bool> Process(List<BaseEventLogModel> validSignals)
        {
            if (validSignals.Count == 0)
                return true;
            return await WriteFlashEventsToDb(validSignals);

        }
        #endregion

    }
}
