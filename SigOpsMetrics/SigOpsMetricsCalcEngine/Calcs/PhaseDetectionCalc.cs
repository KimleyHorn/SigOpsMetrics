using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using SigOpsMetricsCalcEngine.Models;

namespace SigOpsMetricsCalcEngine.Calcs;

public class PhaseDetectionCalc
{
    public static async Task<List<BaseEventLogModel>> RunPhase(List<DateTime> validDates, List<BaseEventLogModel> sigModels)
    { 
        var filteredData = PhaseDetectionDataAccessLayer.FilterMissedOrOmitted(sigModels);
        return filteredData;
    }

    public static void FindPercentUptime(List<BaseEventLogModel> sigModels, List<BaseEventLogModel> filteredData)
    {
        //Just making sure logic is there, 100% will have to clean this code up and make prettier
        throw new NotImplementedException();
    }
}