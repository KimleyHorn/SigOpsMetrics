using System.Collections.Concurrent;
using System.Text;
using Amazon.Runtime.Internal.Transform;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using SigOpsMetricsCalcEngine.Models;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace SigOpsMetricsCalcEngine.Calcs;

public class PhaseDetectionCalc
{
    private static List<string> _stringDataCollection = new List<string>();
    private static List<PhaseDetectionModel> _downtimeCollection = new List<PhaseDetectionModel>();

    private static readonly string _fileDirectory = ConfigurationManager.AppSettings["PHASE_OUTPUT_DIRECTORY"];
    public static async Task<List<string>> RunPhase(List<DateTime> validDates, List<BaseEventLogModel> sigModels, List<long?> regionCodes)
    {
        foreach (var date in validDates)
        {
            var tasks = new List<Task>();
            var lockObject = new object();

            foreach (var signalId in regionCodes)
            {
                tasks.Add(Task.Run(() =>
                {
                    var signalData = sigModels.Where(x =>
                        x.SignalID == signalId &&
                        x.Timestamp.Hour >= 7 &&
                        x.Timestamp.Hour <= 17).ToList();

                    var filteredData = PhaseDetectionDataAccessLayer.FilterMissedOrOmitted(signalData, signalId);

                    lock (lockObject)
                    {
                        var percentDowntime = CalculateDowntime(signalData.Count, filteredData.Count);
                        AddToCollections(date, signalId, percentDowntime);
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
            SortDowntimeCollection();
            WritePhaseDetectionToCSV(date);
            _downtimeCollection = new List<PhaseDetectionModel>();
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
            csvContent.AppendLine("Date,SignalID,Downtime");

            //Writing Objects to CSV
            foreach (var signalInfo in _downtimeCollection.Where(x => x.Date == date))
            {
                var line = $"{signalInfo.Date.ToShortDateString()},{signalInfo.SignalID},{signalInfo.Downtime}%";
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
        _downtimeCollection = _downtimeCollection.OrderBy(x => x.SignalID).ToList();
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

    private static void AddToCollections(DateTime date, long? signalId, float? percentDowntime)
    {
        _downtimeCollection.Add(new PhaseDetectionModel
        {
            Date = date,
            Downtime = percentDowntime,
            SignalID = signalId
        });
        _stringDataCollection.Add($"Date: {date.ToShortDateString()} - SignalID: {signalId} - Downtime: {percentDowntime}%");
    }
    #endregion

}