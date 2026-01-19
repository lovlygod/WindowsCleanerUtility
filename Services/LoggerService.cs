using System;
using Serilog;
using Serilog.Core;

namespace WindowsCleanerUtility.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly ILogger _logger;

        public LoggerService()
        {
            _logger = new LoggerConfiguration()
                .WriteTo.File("logs/cleaner-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public void Log(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    _logger.Verbose(message);
                    break;
                case LogLevel.Debug:
                    _logger.Debug(message);
                    break;
                case LogLevel.Info:
                    _logger.Information(message);
                    break;
                case LogLevel.Warning:
                    _logger.Warning(message);
                    break;
                case LogLevel.Error:
                    _logger.Error(message);
                    break;
                case LogLevel.Fatal:
                    _logger.Fatal(message);
                    break;
            }
        }

        public void LogTrace(string message) => _logger.Verbose(message);
        public void LogDebug(string message) => _logger.Debug(message);
        public void LogInfo(string message) => _logger.Information(message);
        public void LogWarning(string message) => _logger.Warning(message);
        public void LogError(string message) => _logger.Error(message);
        public void LogFatal(string message) => _logger.Fatal(message);
        
        public void LogException(Exception exception, string? message = null)
        {
            if (message != null)
                _logger.Error(exception, message);
            else
                _logger.Error(exception, exception.Message);
        }
    }
}