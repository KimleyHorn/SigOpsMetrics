using SigOpsMetricsCalcEngine.Models;
using System.Configuration;
using System.Data;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    public class FlashEventDataAccessLayer : BaseDataAccessLayer, IDataAccess
    {
        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["FLASH_EVENT_TABLE_NAME"] ?? "flash_event_log";
        internal static readonly List<long?> EventList = [173];

        /// <summary>
        /// Constructor for the FlashEventDataAccessLayer that takes in a list of signals
        /// </summary>
        /// <param name="sigModels">A list of signals represented by BaseEventLogModels</param>
        public FlashEventDataAccessLayer(List<BaseEventLogModel> sigModels)
        {
            SignalEvents = sigModels;
        }

        #region Write to MySQL

        /// <summary>
        /// A method that writes all the filtered BaseEventLogModels to the MySQL database
        /// </summary>
        /// <param name="preempts">An enumerable of BaseEventLogModels that will be written to the MySQL database</param>
        /// <returns>True if the operation is successful, false otherwise</returns>
        /// <exception cref="InvalidOperationException">An exception thrown when the MySQL writer fails</exception>
        public static async Task<bool> WriteFlashEventsToDb(IEnumerable<BaseEventLogModel> events)
        {
            // Create a DataTable to hold the events data
            var dataTable = new DataTable();
            dataTable.Columns.Add("Timestamp", typeof(DateTime));
            dataTable.Columns.Add("signalID", typeof(long));
            dataTable.Columns.Add("EventCode", typeof(long));
            dataTable.Columns.Add("EventParam", typeof(long));

            foreach (var eventData in events)
            {
                dataTable.Rows.Add(
                    eventData.Timestamp,
                    eventData.SignalID,
                    eventData.EventCode,
                    eventData.EventParam

                );
            }

            try
            {
                return await MySqlWriter(MySqlTableName ?? throw new InvalidOperationException(), dataTable);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                await WriteToErrorLog("FlashEventDataAccessLayer", "toMySQL", e);
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

        #region Driver Method

        /// <summary>
        /// Driver method for the flash event processing
        /// </summary>
        /// <param name="validSignals">A list of signals that will be written to the MySQL database</param>
        /// <returns> true if the database is written to or if there are no signals to write to the database</returns>
        public async Task<bool> Process(List<BaseEventLogModel> validSignals)
        {
            try
            {
                if (validSignals.Count == 0)
                    return true;
                return await WriteFlashEventsToDb(validSignals);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                await WriteToErrorLog("FlashEventDataAccessLayer", "Process", e);
                throw;
            }
        }

        #endregion Driver Method
    }
}