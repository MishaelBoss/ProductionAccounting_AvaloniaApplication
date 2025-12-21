using System;
using System.IO;
using System.Threading;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

internal abstract class Loges
{
    private static readonly Lock LockObj = new Lock();
    private static string? _currentLogDate;
    private static string? _currentLogPath;

    public static void ExceptionEventApp(object sender, UnhandledExceptionEventArgs ueea)
    {
        EnsureLogDirectory();

        if (ueea.ExceptionObject is Exception e) LoggingProcess(LogLevel.Error, e.ToString());
    }

    public static void LoggingProcess(LogLevel level, string? message = null, Exception? ex = null)
    {
        lock (LockObj)
        {
            EnsureLogDirectory();
            string todayDate = DateTime.Today.ToString("yyyy-MM-dd");

            switch (level)
            {
                case LogLevel.Info when Arguments.Loggers.LoggerInfo:
                case LogLevel.Debug when Arguments.Loggers.LoggerDebug:
                case LogLevel.Warning when Arguments.Loggers.LoggerWarning:
                case LogLevel.Error when Arguments.Loggers.LoggerError:
                case LogLevel.Critical when Arguments.Loggers.LoggerCritical:
                    if (ShouldCreateNewLog(todayDate)) CreateNewLogFile(todayDate);

                    WriteLogEntry(level, message ?? string.Empty, ex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
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
        var entry = $"[{DateTime.Now:HH:mm:ss}]-[{level}:] {message} - index page error - {GetPageInfo(ex)} {Environment.NewLine}";

        if (!File.Exists(_currentLogPath)) return;

        File.AppendAllText(_currentLogPath, entry);
    }

    private static string GetPageInfo(Exception? ex)
    {
        return ex is { StackTrace: not null } ? GetPageFromStackTrace(ex.StackTrace) : "Информация о странице не доступна";
    }

    private static string GetPageFromStackTrace(string stackTrace)
    {
        return stackTrace;
    }
}

public enum LogLevel
{
    Info = 0,
    Debug = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}