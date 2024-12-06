using System.Collections.Concurrent;
using System.Configuration;
using System.Text;
using Parquet;
using Parquet.Data;
using SigOpsMetricsCalcEngine.Core.Models;

namespace SigOpsMetricsCalcEngine.Core.DataAccess
{
    internal class RampMeterDataAccessLayer : BaseDataAccessLayer

    {
        private static readonly string? MySqlTableName = ConfigurationManager.AppSettings["CYCLE_TIME_TABLE_NAME"] ?? "ramp_meter_log";
        internal static readonly List<long?> EventList = [131];
        private static readonly string? FilePath = ConfigurationManager.AppSettings["RAMP_FILE_PATH"] ?? @"C:\Development\SigOpsMetrics\Ramp_Meters";
        private static readonly string? InputPath = ConfigurationManager.AppSettings["INPUT_PATH"] ?? string.Empty;
        /// <summary>
        /// Constructor for the RampMeterDataAccessLayer that takes in a list of signals
        /// </summary>
        /// <param name="sigModels">A list of signals represented by BaseEventLogModels</param>
        public RampMeterDataAccessLayer(ConcurrentBag<BaseEventLogModel> sigModels)
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
        #endregion

        public async Task<bool> UnpackParquetFilesAsync(string inputDirectoryPath, string outputDirectoryPath, string outputFileName)
        {
            if (!Directory.Exists(inputDirectoryPath))
            {
                Console.WriteLine("Directory does not exist: " + inputDirectoryPath);
                return false;
            }

            if (!Directory.Exists(outputDirectoryPath))
            {
                Directory.CreateDirectory(outputDirectoryPath);
            }

            string outputFilePath = Path.Combine(outputDirectoryPath, outputFileName);

            var files = Directory.GetFiles(inputDirectoryPath, "ramp_meter_events_*.parquet")
                                 .OrderBy(f => f)
                                 .Take(10)
                                 .ToList();

            if (!files.Any())
            {
                Console.WriteLine("No matching files found in directory: " + inputDirectoryPath);
                return false;
            }

            try
            {
                await using var writer = new StreamWriter(outputFilePath);

                bool headersWritten = false;

                foreach (var inputFilePath in files)
                {
                    try
                    {
                        await using Stream fileStream = File.OpenRead(inputFilePath);
                        var options = new ParquetOptions { TreatByteArrayAsString = true };
                        using var parquetReader = await ParquetReader.CreateAsync(fileStream, options);

                        // Write CSV headers if not already written
                        if (!headersWritten)
                        {
                            var columns = parquetReader.Schema.Fields.Select(f => f.Name).ToArray();
                            await writer.WriteLineAsync(string.Join(",", columns));
                            headersWritten = true;
                        }

                        // Read all rows from the Parquet file
                        for (int i = 0; i < parquetReader.RowGroupCount; i++)
                        {
                            using var rowGroupReader = parquetReader.OpenRowGroupReader(i);
                            var dataFields = parquetReader.Schema.GetDataFields();
                            var columnData = new DataColumn[dataFields.Length];

                            for (int j = 0; j < dataFields.Length; j++)
                                columnData[j] = await rowGroupReader.ReadColumnAsync(dataFields[j]);

                            int rowCount = columnData[0].Data.Length;

                            for (int row = 0; row < rowCount; row++)
                            {
                                var rowValues = columnData.Select(c =>
                                {
                                    if (c.Field.Name == "timestamp" && c.Data.GetValue(row) is DateTime timestamp)
                                    {
                                        return timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                                    }
                                    return c.Data.GetValue(row)?.ToString();
                                }).ToArray();
                                await writer.WriteLineAsync(string.Join(",", rowValues));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to process file {inputFilePath}: {ex.Message}");
                        return false;
                    }
                }

                Console.WriteLine("Data successfully written to CSV file: " + outputFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create or write to output file: {ex.Message}");
                return false;
            }

            return true;
        }



        /// <summary>
        /// A method that filters a list of BaseEventLogModels by valid dates
        /// </summary>
        /// <param name="startDate">The first date the filter looks at</param>
        /// <param name="endDate">The last date the filter looks at</param>
        /// <returns>A filtered list of BaseEventLogModels</returns>
        public async Task<ConcurrentBag<BaseEventLogModel>> Filter(DateTime startDate, DateTime endDate)
        {
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                         .Select(offset => startDate.AddDays(offset)).ToList();
            var validData = new ConcurrentBag<BaseEventLogModel>(await FilterData(allDates, EventList, MySqlTableName ?? " ", "Timestamp"));
            return validData;
        }


        public async Task<bool> Process(ConcurrentBag<BaseEventLogModel>? isFiltered = null)
        {
            try
            {
                //if (isFiltered.Count == 0)
                //    return false;
                //return await WriteRampMetersToCSV(FilePath ?? " ", isFiltered);
                return await UnpackParquetFilesAsync(InputPath ?? " ", FilePath ?? " ", "testOutput.csv");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error" + e);
                //await WriteToErrorLog("FlashEventDataAccessLayer", "Process", e);
                throw;
            }
        }
    }
}
