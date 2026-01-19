using Microsoft.Extensions.DependencyInjection;
using WindowsCleanerUtility.Settings;

namespace WindowsCleanerUtility.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCleanerServices(this IServiceCollection services, UserSettings settings)
                {
                    services.AddSingleton<ILoggerService, LoggerService>();
                    
                    services.AddScoped<GlobalExceptionHandler>();
                    
                    services.AddScoped<IFileOperations, FileOperationsService>();
                    
                    services.AddScoped<ICleanerService, TemporaryFilesCleaner>();
                    services.AddScoped<ICleanerService, BrowserDataCleaner>();
                    services.AddScoped<ICleanerService, SystemLogsCleaner>();
                    services.AddScoped<ICleanerService, OldFilesCleaner>();
                    services.AddScoped<ICleanerService, DNSCacheCleaner>();
                    
                    services.AddScoped<CleaningManager>();
                    
                    services.AddScoped<ISchedulerService, SchedulerService>();
                    services.AddScoped<IReportService, ReportService>();
                    
                    return services;
                }
    }
}