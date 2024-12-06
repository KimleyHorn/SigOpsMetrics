using System.Text;

namespace SigOpsMetricsCalcEngine.Core.Extensions;

public enum LogLevel
{
    Error,
    Warning,
    Info,
    Debug
}

public class ErrorLogger : IErrorLogger
{
    private readonly SemaphoreSlim _logWriteSemaphore = new SemaphoreSlim(1, 1);
    private readonly string _logDirectoryPath;
    private readonly string _logFilePath;
    private readonly long _maxLogFileSizeInBytes;
    private readonly LogLevel _minimumLogLevel;

    public ErrorLogger(string logDirectoryPath, long maxLogFileSizeInBytes = 5 * 1024 * 1024, LogLevel minimumLogLevel = LogLevel.Info)
    {
        if (string.IsNullOrWhiteSpace(logDirectoryPath))
        {
            throw new ArgumentNullException(nameof(logDirectoryPath), "Log directory path must be provided.");
        }

        _logDirectoryPath = logDirectoryPath;
        _logFilePath = Path.Combine(logDirectoryPath, "errorlog.txt");
        _maxLogFileSizeInBytes = maxLogFileSizeInBytes;  // Default to 5 MB
        _minimumLogLevel = minimumLogLevel;

        // Ensure the log directory exists
        Directory.CreateDirectory(logDirectoryPath);
    }

    /// <summary>
    /// Writes an exception to the error log asynchronously with the specified log level.
    /// </summary>
    /// <param name="applicationName">The name of the application or component where the error occurred.</param>
    /// <param name="functionName">The name of the function where the error occurred.</param>
    /// <param name="ex">The exception to log.</param>
    /// <param name="logLevel">The log level of the message.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task WriteToErrorLogAsync(string applicationName, string functionName, Exception ex, LogLevel logLevel = LogLevel.Error)
    {
        if (ex == null)
        {
            throw new ArgumentNullException(nameof(ex), "Exception cannot be null.");
        }

        // Check if the log level meets the minimum log level
        if (logLevel > _minimumLogLevel)
        {
            return;  // Don't log if the log level is below the minimum threshold
        }

        // Build the error message
        var errorMessage = BuildErrorMessage(applicationName, functionName, ex, logLevel);

        // Write to the error log file asynchronously
        await WriteErrorToFileAsync(errorMessage);
    }

    /// <summary>
    /// Builds a detailed error message from the provided exception with a log level.
    /// </summary>
    /// <param name="applicationName">The name of the application or component where the error occurred.</param>
    /// <param name="functionName">The name of the function where the error occurred.</param>
    /// <param name="ex">The exception to log.</param>
    /// <param name="logLevel">The log level of the message.</param>
    /// <returns>A formatted string representing the error message.</returns>
    public string BuildErrorMessage(string applicationName, string functionName, Exception ex, LogLevel logLevel)
    {
        var sb = new StringBuilder();
        sb.AppendLine("------------------------------------------------------------");
        sb.AppendLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
        sb.AppendLine($"Log Level: {logLevel}");
        sb.AppendLine($"Application: {applicationName}");
        sb.AppendLine($"Function: {functionName}");
        sb.AppendLine($"Exception Type: {ex.GetType().FullName}");
        sb.AppendLine($"Message: {ex.Message}");
        if (ex.InnerException != null)
        {
            sb.AppendLine($"Inner Exception: {ex.InnerException.Message}");
        }
        sb.AppendLine($"Stack Trace: {ex.StackTrace}");
        sb.AppendLine("------------------------------------------------------------");
        return sb.ToString();
    }

    /// <summary>
    /// Writes the error message to a log file asynchronously in a thread-safe manner, with log rotation if needed.
    /// </summary>
    /// <param name="errorMessage">The error message to write to the log file.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task WriteErrorToFileAsync(string errorMessage)
    {
        await _logWriteSemaphore.WaitAsync();
        try
        {
            // Rotate log file if it exceeds the max file size
            if (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length > _maxLogFileSizeInBytes)
            {
                RotateLogFile();
            }

            // Append the error message to the log file asynchronously
            await File.AppendAllTextAsync(_logFilePath, errorMessage);
        }
        finally
        {
            _logWriteSemaphore.Release();
        }
    }

    /// <summary>
    /// Rotates the current log file by renaming it with a timestamp and creates a new log file.
    /// </summary>
    public void RotateLogFile()
    {
        var archiveFileName = $"errorlog_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";
        var archiveFilePath = Path.Combine(_logDirectoryPath, archiveFileName);

        // Rename the current log file
        File.Move(_logFilePath, archiveFilePath);
    }

    /// <summary>
    /// Implements IDisposable to ensure that the semaphore is properly disposed of.
    /// </summary>
    public void Dispose()
    {
        _logWriteSemaphore?.Dispose();
    }
}

//class DebugLogger
//{
//    public DebugLogger()
//    {

//    }
//}