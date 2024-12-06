using System.Collections.Concurrent;
using System.Text;
using SigOpsMetricsCalcEngine.Core.DataAccess;
using SigOpsMetricsCalcEngine.Core.Models;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace SigOpsMetricsCalcEngine.Core.Calcs;

public class PhaseDetectionCalc
{
    private static List<string> _stringDataCollection = [];
    private static ConcurrentBag<PhaseDetectionModel> _downtimeCollection = [];
    //Todo force use of dynamic file directory
    private static readonly string _fileDirectory = ConfigurationManager.AppSettings["PHASE_OUTPUT_DIRECTORY"];
    public static async Task<List<string>> RunPhase(List<DateTime> validDates, ConcurrentBag<BaseEventLogModel> sigModels, List<long?> regionCodes)
    {
        foreach (var date in validDates)
        {
            var lockObject = new object();

            Task.WaitAll(regionCodes.Select((signalId, phase) => Task.Run(() =>
            {
                var signalData = sigModels.Where(x => x.SignalID == signalId && x.EventParam == phase && x.Timestamp.Hour >= 7 && x.Timestamp.Hour <= 17).ToList();

                var filteredData = PhaseDetectionDataAccessLayer.FilterMissedOrOmitted(signalData, signalId);

                //lock (lockObject)
                //{
                    var percentDowntime = CalculateDowntime(signalData.Count, filteredData.Count);
                    AddToCollections(date, signalId, percentDowntime, phase);
                //}
            })).ToArray());
            SortDowntimeCollection();
            WritePhaseDetectionToCSV(date);
            _downtimeCollection = [];
        }
        SortStringCollection();
        return _stringDataCollection;
    }

    #region WriteToCSV
    private static async void WritePhaseDetectionToCSV(DateTime date)
    {
        try
        {
            if (!Directory.Exists(_fileDirectory))
            {
                Directory.CreateDirectory(_fileDirectory); // Create the directory if it does not exist
            }

            var csvContent = new StringBuilder();
            //Header Line 
            csvContent.AppendLine("Date,SignalID,Phase,Downtime");

            //Writing Objects to CSV
            foreach (var signalInfo in _downtimeCollection.Where(x => x.Date == date))
            {
                var line = $"{signalInfo.Date.ToShortDateString()},{signalInfo.SignalID},{signalInfo.Phase},{signalInfo.Downtime}%";
                csvContent.AppendLine(line);
            }

            var fileDate = date.ToString("yyyy-M-dd");
            var filePath = _fileDirectory + $@"\PhaseDetectionCalc_{fileDate}.csv";
            await File.WriteAllTextAsync(filePath, csvContent.ToString());
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine("Error: Access to the path is denied. " + ex.Message);

        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine("Error: Directory not found. " + ex.Message);
        }
        catch (IOException ex)
        {
            Console.WriteLine("Error: IO exception. " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: An unexpected error occurred. " + ex.Message);
        }
    }
    #endregion

    #region Sorting
    private static void SortDowntimeCollection()
    {
        _downtimeCollection = new ConcurrentBag<PhaseDetectionModel>(_downtimeCollection.OrderBy(x => x.SignalID).ToList());
    }

    private static void SortStringCollection()
    {
        _stringDataCollection = _stringDataCollection.OrderBy(x => ExtractIDFromString(x)).ToList();
    }

    private static int ExtractIDFromString(string str)
    {
        int idIndex = str.IndexOf("SignalID: ") + "SignalID: ".Length;
        int endIndex = str.IndexOf(" - Downtime:", idIndex);
        string idString = str.Substring(idIndex, endIndex - idIndex);
        return int.Parse(idString);
    }
    #endregion

    #region DataHelp
    private static float? CalculateDowntime(int? signalData, int? filteredData)
    {
        if (!signalData.HasValue) throw new DivideByZeroException();
        var percentDowntime = 100 - (filteredData * 100f / signalData);
        return percentDowntime;
    }

    private static void AddToCollections(DateTime date, long? signalId, float? percentDowntime, long phase)
    {
        _downtimeCollection.Add(new PhaseDetectionModel
        {
            Date = date,
            Downtime = percentDowntime,
            SignalID = signalId,
            Phase = phase
        });
        _stringDataCollection.Add($"Date: {date.ToShortDateString()} - SignalID: {signalId} - Downtime: {percentDowntime}%");
    }
    #endregion

}