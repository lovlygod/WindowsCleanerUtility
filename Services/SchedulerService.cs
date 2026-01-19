using System;
using System.Threading;
using System.Threading.Tasks;
using ThreadingTimer = System.Threading.Timer;

namespace WindowsCleanerUtility.Services
{
    public class SchedulerService : ISchedulerService, IDisposable
    {
        private ThreadingTimer _timer;
        private readonly CleaningManager _cleaningManager;
        private readonly ILoggerService _logger;
        private CleaningOptions _options;
        
        public bool IsScheduled { get; private set; }
        
        public SchedulerService(CleaningManager cleaningManager, ILoggerService logger)
        {
            _cleaningManager = cleaningManager;
            _logger = logger;
        }
        
        public void ScheduleCleanup(CleaningOptions options, int hoursInterval)
        {
            _options = options;
            var interval = TimeSpan.FromHours(hoursInterval);
            
            _timer = new ThreadingTimer(async _ => await DoScheduledCleanup(), null, interval, interval);
            IsScheduled = true;
            
            _logger.LogInfo($"Scheduled cleanup every {hoursInterval} hours");
        }
        
        public void CancelSchedule()
        {
            _timer?.Dispose();
            IsScheduled = false;
            
            _logger.LogInfo("Scheduled cleanup cancelled");
        }
        
        private async Task DoScheduledCleanup()
        {
            try
            {
                _logger.LogInfo("Starting scheduled cleanup");
                await _cleaningManager.PerformCleaningAsync(_options, CancellationToken.None);
                _logger.LogInfo("Scheduled cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in scheduled cleanup: {ex.Message}");
            }
        }
        
        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}