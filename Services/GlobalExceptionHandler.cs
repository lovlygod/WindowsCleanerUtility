using System;
using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public class GlobalExceptionHandler
    {
        private readonly ILoggerService _logger;
        
        public GlobalExceptionHandler(ILoggerService logger)
        {
            _logger = logger;
        }
        
        public async Task<T> HandleAsync<T>(Func<Task<T>> operation, string operationName)
                {
                    try
                    {
                        return await operation();
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInfo($"{operationName} was cancelled");
                        return default(T);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in {operationName}: {ex.Message}");
                        throw new CleaningException($"Failed to execute {operationName}", ex);
                    }
                }
                
                public async Task HandleAsync(Func<Task> operation, string operationName)
                {
                    try
                    {
                        await operation();
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInfo($"{operationName} was cancelled");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in {operationName}: {ex.Message}");
                        throw new CleaningException($"Failed to execute {operationName}", ex);
                    }
                }
    }
    
    public class CleaningException : Exception
    {
        public CleaningException(string message) : base(message)
        {
        }
        
        public CleaningException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}