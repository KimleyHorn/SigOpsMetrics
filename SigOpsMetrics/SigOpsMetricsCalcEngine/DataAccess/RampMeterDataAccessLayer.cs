using SigOpsMetricsCalcEngine.Models;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Parquet;
using Parquet.Schema;

namespace SigOpsMetricsCalcEngine.DataAccess
{
    internal class RampMeterDataAccessLayer : BaseDataAccessLayer, IDataAccess

    {
        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["CYCLE_TIME_TABLE_NAME"] ?? "ramp_meter_log";
        internal static readonly List<long?> EventList = [131];
        private static readonly string? FilePath = ConfigurationManager.AppSettings["FILE_PATH"] ?? @"C:\Development\SigOpsMetrics\Ramp_Meters";
        private static readonly string? InputPath = ConfigurationManager.AppSettings["INPUT_PATH"] ?? string.Empty;
        /// <summary>
        /// Constructor for the RampMeterDataAccessLayer that takes in a list of signals
        /// </summary>
        /// <param name="sigModels">A list of signals represented by BaseEventLogModels</param>
        public RampMeterDataAccessLayer(List<BaseEventLogModel> sigModels)
        {
            SignalEvents = sigModels;
        }

        #region Write to CSV

        public static async Task<bool> WriteRampMetersToCSV(string dirPath, IEnumerable<BaseEventLogModel> events)
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
                var filePath = dirPath + @$"\RampMeterRawData_{fileDate:yyyy-MM-dd}.csv";
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
        public async Task<bool> UnpackParquetFileAsync(string inputDirectoryPath, string outputDirectoryPath)
        {
            

            if (!File.Exists(inputDirectoryPath))
            {
                Console.WriteLine("File does not exist: " + inputDirectoryPath);
                return false;
            }


            string outputFilePath = outputDirectoryPath;

            await using Stream fileStream = File.OpenRead(inputDirectoryPath);
            var options = new ParquetOptions {TreatByteArrayAsString = true};
            var parquetReader = await ParquetReader.CreateAsync(fileStream, options);

            await using var writer = new StreamWriter(outputFilePath);

            // Write CSV headers
            await writer.WriteLineAsync("timestamp,signalID,eventcode,eventparam");
            // Read all rows from the Parquet file
            foreach (var row in await parquetReader.ReadEntireRowGroupAsync(0))
            {
                writer.WriteLine(row.ToString());
            }
            Console.WriteLine("Data successfully written to CSV file: " + outputFilePath);
            return true;
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


        public async Task<bool> Process(List<BaseEventLogModel> isFiltered = null)
        {
            try
            {
                //if (isFiltered.Count == 0)
                //    return false;
                //return await WriteRampMetersToCSV(FilePath ?? " ", isFiltered);
                return await UnpackParquetFileAsync(InputPath ?? " ", FilePath ?? " ");
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
