using System;

namespace WindowsCleanerUtility.Services
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public interface ILoggerService
    {
        void Log(LogLevel level, string message);
        void LogTrace(string message);
        void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogFatal(string message);
        void LogException(Exception exception, string? message = null);
    }
}