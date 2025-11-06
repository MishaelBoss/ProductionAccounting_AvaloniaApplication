using System;
using System.IO;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

internal abstract class Loges
{
    private static readonly object _lockObj = new object();
    private static string? _currentLogDate;
    private static string? _currentLogPath;

    public static void ExceptionEventApp(object sender, UnhandledExceptionEventArgs ueea)
    {
        EnsureLogDirectory();

        if (ueea.ExceptionObject is Exception e) LoggingProcess(LogLevel.ERROR, e.ToString());
    }

    public static void LoggingProcess(LogLevel level, string? message = null, Exception? ex = null)
    {
        lock (_lockObj)
        {
            EnsureLogDirectory();
            string todayDate = DateTime.Today.ToString("yyyy-MM-dd");

            switch (level)
            {
                case LogLevel.INFO when Arguments.Loggers.LoggerInfo:
                    if (ShouldCreateNewLog(todayDate)) CreateNewLogFile(todayDate);

                    WriteLogEntry(level, message ?? string.Empty, ex);
                    break;
                case LogLevel.DEBUG when Arguments.Loggers.LoggerDebug:
                    if (ShouldCreateNewLog(todayDate)) CreateNewLogFile(todayDate);

                    WriteLogEntry(level, message ?? string.Empty, ex);
                    break;
                case LogLevel.WARNING when Arguments.Loggers.LoggerWarning:
                    if (ShouldCreateNewLog(todayDate)) CreateNewLogFile(todayDate);

                    WriteLogEntry(level, message ?? string.Empty, ex);
                    break;
                case LogLevel.ERROR when Arguments.Loggers.LoggerError:
                    if (ShouldCreateNewLog(todayDate)) CreateNewLogFile(todayDate);

                    WriteLogEntry(level, message ?? string.Empty, ex);
                    break;
                case LogLevel.CRITICAL when Arguments.Loggers.LoggerCritical:
                    if (ShouldCreateNewLog(todayDate)) CreateNewLogFile(todayDate);

                    WriteLogEntry(level, message ?? string.Empty, ex);
                    break;
            }
        }
    }

    private static void EnsureLogDirectory()
    {
        if (!Directory.Exists(Paths.Log)) Directory.CreateDirectory(Paths.Log);
    }

    private static bool ShouldCreateNewLog(string todayDate)
    {
        return _currentLogDate != todayDate ||
               string.IsNullOrEmpty(_currentLogPath) ||
               !File.Exists(_currentLogPath);
    }

    private static void CreateNewLogFile(string date)
    {
        _currentLogPath = Path.Combine(Paths.Log, $"log-{date}.txt");
        _currentLogDate = date;

        if (!File.Exists(_currentLogPath)) File.WriteAllText(_currentLogPath, $"=== Log started {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\n\n");
    }

    private static void WriteLogEntry(LogLevel level, string? message = null, Exception? ex = null)
    {
        string entry = $"[{DateTime.Now:HH:mm:ss}]-[{level}:] {message} - index page error - {GetPageInfo(ex)} {Environment.NewLine}";

        if (!File.Exists(_currentLogPath)) return;

        File.AppendAllText(_currentLogPath, entry);
    }

    private static string GetPageInfo(Exception? ex)
    {
        if (ex != null && ex.StackTrace != null)
        {
            return GetPageFromStackTrace(ex.StackTrace);
        }
        return "Информация о странице не доступна";
    }

    private static string GetPageFromStackTrace(string stackTrace)
    {
        return stackTrace;
    }
}

public enum LogLevel
{
    INFO = 0,
    DEBUG = 1,
    WARNING = 2,
    ERROR = 3,
    CRITICAL = 4
}