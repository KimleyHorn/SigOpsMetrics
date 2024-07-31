using System.Text;
using Amazon.Runtime.Internal.Transform;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs;

public class PhaseDetectionCalc
{
    public static List<string> StringDataCollection = new List<string>();
    public static List<PhaseDetectionModel> DowntimeCollection = new List<PhaseDetectionModel>();

    private static string _fileDirectory = "C:\\Users\\alex.valentin\\source\\repos\\SigOpsMetricsCSV";
    public static async Task<List<string>> RunPhase(List<DateTime> validDates, List<BaseEventLogModel> sigModels, List<long?> regionCodes)
    {
        var filteredData = new List<BaseEventLogModel>();
        var signalData = new List<BaseEventLogModel>();

        foreach (var date in validDates)
        {
            foreach (var signalId in regionCodes)
            {
                signalData = sigModels.Where(x => (x.SignalID == signalId) &&
                                                  (x.Timestamp.Hour >= 7 && x.Timestamp.Hour <= 17)).ToList();
                filteredData = PhaseDetectionDataAccessLayer.FilterMissedOrOmitted(signalData, signalId);
                var percentDowntime = CalculateDowntime(signalData.Count, filteredData.Count);
                AddToCollections(date, signalId, percentDowntime);
            }
            WritePhaseDetectionToCSV(date);
            DowntimeCollection = new List<PhaseDetectionModel>();
        }
        return StringDataCollection;
    }

    public static float? CalculateDowntime(int? signalData, int? filteredData)
    {
        if (!signalData.HasValue) throw new DivideByZeroException();
        var percentDowntime = 100 - (filteredData * 100f / signalData);
        return percentDowntime;
    }

    public static void AddToCollections(DateTime date, long? signalId, float? percentDowntime)
    {
        DowntimeCollection.Add(new PhaseDetectionModel
        {
            Date = date,
            Downtime = percentDowntime,
            SignalID = signalId
        });
        StringDataCollection.Add($"Date: {date.ToShortDateString()} - SignalID: {signalId} - Downtime: {percentDowntime}%");
    }

    public static async void WritePhaseDetectionToCSV(DateTime date)
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
            foreach (var signalInfo in DowntimeCollection.Where(x => x.Date == date))
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
}