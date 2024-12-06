namespace SigOpsMetricsCalcEngine.Core.Extensions;

public interface IErrorLogger : IDisposable
{

    Task WriteToErrorLogAsync(string applicationName, string functionName, Exception ex,
        LogLevel logLevel = LogLevel.Error);

    string BuildErrorMessage(string applicationName, string functionName, Exception ex, LogLevel logLevel);

    Task WriteErrorToFileAsync(string errorMessage);

    void RotateLogFile();

    void Dispose();
}