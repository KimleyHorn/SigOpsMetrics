using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    internal class CycleTimeDataAccessLayer : BaseDataAccessLayer, IDataAccess
    {
        internal static readonly List<long?> EventList = [131];
        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["CYCLE_TIME_TABLE_NAME"] ?? "cycle_time_log";

        private static readonly string? FilePath =
            ConfigurationManager.AppSettings["FILE_PATH"] ?? @"C:\Development\SigOpsMetrics\Cycle_Times;";

        public CycleTimeDataAccessLayer(List<BaseEventLogModel> sigModels)
        {
            SignalEvents = sigModels;
        }

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

        #region Write to CSV

        public static async Task<bool> WriteCycleTimeToCSV(string dirPath, IEnumerable<BaseEventLogModel> events)
        {
            try
            {
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(dirPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory); // Create the directory if it does not exist
                }

                var csvContent = new StringBuilder();

                // Write the header line
                csvContent.AppendLine("Timestamp,SignalID,EventCode,EventParam");

                // Write each event log entry
                foreach (var log in events)
                {
                    var line = $"{log.Timestamp:yyyy-MM-ddHH:mm:ss.fff},{log.SignalID},{log.EventCode},{log.EventParam}";
                    csvContent.AppendLine(line);
                }
                var fileDate = events.First().Timestamp;
                var filePath = dirPath + @$"\CycleTimeRawData_{fileDate:yyyy-MM-dd}.csv";
                // Write the content to the specified file. This will create a new file if it doesn't exist,
                // or overwrite the existing file.
                await File.WriteAllTextAsync(filePath, csvContent.ToString()); // This line creates the file if it doesn't exist
                return true;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Error: Access to the path is denied. " + ex.Message);
                // Handle unauthorized access
                // await WriteToErrorLog("CsvWriter", "WriteCycleTimeToCSV", ex);
                return false;
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine("Error: Directory not found. " + ex.Message);
                // Handle directory not found
                // await WriteToErrorLog("CsvWriter", "WriteCycleTimeToCSV", ex);
                return false;
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error: IO exception. " + ex.Message);
                // Handle other IO exceptions
                // await WriteToErrorLog("CsvWriter", "WriteCycleTimeToCSV", ex);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: An unexpected error occurred. " + ex.Message);
                // Handle all other exceptions
                // await WriteToErrorLog("CsvWriter", "WriteCycleTimeToCSV", ex);
                return false;
            }
        }

        #endregion Write to MySQL

        public static async Task<bool> CalcCycleTimeDifference(List<BaseEventLogModel> baseSignal)
        {
            foreach (var signal in baseSignal)
            {
                var signalId = signal.SignalID;

                try
                {
                    var startCycle = FilterByEventCode(baseSignal, 131);
                    var endCycle = FilterByEventCode(baseSignal, 316);

                    //var CycleTime = new CycleModel(signal.SignalID, signal.Timestamp,)
                }
                catch (Exception ex)
                {
                    await WriteToErrorLog("CycleTimeCalc", "CalcCycleTime", ex);
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> Process(List<BaseEventLogModel> isFiltered)
        {
            try
            {
                if (isFiltered.Count == 0)
                    return false;
                return await WriteCycleTimeToCSV(FilePath ?? " ", isFiltered);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                await WriteToErrorLog("FlashEventDataAccessLayer", "Process", e);
                throw;
            }
        }
    }
}
