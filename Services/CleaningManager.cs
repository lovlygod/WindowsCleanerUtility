using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public class CleaningManager
    {
        private readonly IEnumerable<ICleanerService> _cleanerServices;
        private readonly ILoggerService _logger;

        public CleaningManager(IEnumerable<ICleanerService> cleanerServices, ILoggerService logger)
        {
            _cleanerServices = cleanerServices;
            _logger = logger;
        }

        public async Task<bool> PerformCleaningAsync(CleaningOptions options, CancellationToken cancellationToken = default)
                {
                    _logger.LogInfo("Starting cleaning process with options:");
                    _logger.LogInfo($"- Include temporary files: {options.IncludeTemporaryFiles}");
                    _logger.LogInfo($"- Include log files: {options.IncludeLogFiles}");
                    _logger.LogInfo($"- Include event logs: {options.IncludeEventLogs}");
                    _logger.LogInfo($"- Include old files: {options.IncludeOldFiles}");
                    _logger.LogInfo($"- Include browser history: {options.IncludeBrowserHistory}");
                    _logger.LogInfo($"- Include browser cookies: {options.IncludeBrowserCookies}");
                    _logger.LogInfo($"- Include DNS temp files: {options.IncludeDNSTempFiles}");
        
                    var servicesToRun = new List<ICleanerService>();
        
                    if (options.IncludeTemporaryFiles)
                        servicesToRun.Add(_cleanerServices.First(s => s is TemporaryFilesCleaner));
        
                    if (options.IncludeLogFiles || options.IncludeEventLogs)
                        servicesToRun.Add(_cleanerServices.First(s => s is SystemLogsCleaner));
        
                    if (options.IncludeOldFiles)
                        servicesToRun.Add(_cleanerServices.First(s => s is OldFilesCleaner));
        
                    if (options.IncludeBrowserHistory || options.IncludeBrowserCookies)
                        servicesToRun.Add(_cleanerServices.First(s => s is BrowserDataCleaner));
        
                    if (options.IncludeDNSTempFiles)
                        servicesToRun.Add(_cleanerServices.First(s => s is DNSCacheCleaner));
        
                    bool allSuccess = true;
        
                    var tasks = new List<Task<(ICleanerService service, bool success)>>();
        
                    foreach (var service in servicesToRun)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
        
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                _logger.LogInfo($"Starting cleaning service: {service.Name}");
                                bool success = await service.CleanAsync(cancellationToken);
                                
                                if (!success)
                                {
                                    _logger.LogWarning($"Cleaning service failed: {service.Name}");
                                }
                                else
                                {
                                    _logger.LogInfo($"Cleaning service completed successfully: {service.Name}");
                                }
                                
                                return (service, success);
                            }
                            catch (OperationCanceledException)
                            {
                                _logger.LogInfo($"Cleaning was cancelled during service: {service.Name}");
                                return (service, false);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error in cleaning service {service.Name}: {ex.Message}");
                                return (service, false);
                            }
                        }));
                    }
        
                    var results = await Task.WhenAll(tasks);
        
                    foreach (var result in results)
                    {
                        if (!result.success)
                        {
                            allSuccess = false;
                        }
                    }
        
                    _logger.LogInfo($"Cleaning process completed. Overall success: {allSuccess}");
                    return allSuccess;
                }
        
                public async Task<CleaningResult> PerformCleaningWithReportAsync(CleaningOptions options, CancellationToken cancellationToken = default)
                {
                    var startTime = DateTime.Now;
                    var result = new CleaningResult
                    {
                        StartTime = startTime,
                        ServiceResults = new List<ServiceResult>()
                    };
        
                    _logger.LogInfo("Starting cleaning process with reporting:");
        
                    var servicesToRun = new List<ICleanerService>();
        
                    if (options.IncludeTemporaryFiles)
                        servicesToRun.Add(_cleanerServices.First(s => s is TemporaryFilesCleaner));
        
                    if (options.IncludeLogFiles || options.IncludeEventLogs)
                        servicesToRun.Add(_cleanerServices.First(s => s is SystemLogsCleaner));
        
                    if (options.IncludeOldFiles)
                        servicesToRun.Add(_cleanerServices.First(s => s is OldFilesCleaner));
        
                    if (options.IncludeBrowserHistory || options.IncludeBrowserCookies)
                        servicesToRun.Add(_cleanerServices.First(s => s is BrowserDataCleaner));
        
                    if (options.IncludeDNSTempFiles)
                        servicesToRun.Add(_cleanerServices.First(s => s is DNSCacheCleaner));
        
                    bool allSuccess = true;
        
                    var tasks = new List<Task<ServiceResult>>();
        
                    foreach (var service in servicesToRun)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
        
                        tasks.Add(Task.Run(async () =>
                        {
                            var serviceStartTime = DateTime.Now;
                            var serviceResult = new ServiceResult
                            {
                                ServiceName = service.Name,
                                StartTime = serviceStartTime
                            };
        
                            try
                            {
                                _logger.LogInfo($"Starting cleaning service: {service.Name}");
                                
                                bool success = await service.CleanAsync(cancellationToken);
                                
                                var serviceEndTime = DateTime.Now;
                                serviceResult.EndTime = serviceEndTime;
                                serviceResult.Success = success;
                                serviceResult.SpaceFreed = success ? service.SpaceFreed : 0;
                                serviceResult.FilesProcessed = success ? service.FilesProcessed : 0;
                                
                                if (!success)
                                {
                                    _logger.LogWarning($"Cleaning service failed: {service.Name}");
                                    allSuccess = false;
                                }
                                else
                                {
                                    _logger.LogInfo($"Cleaning service completed successfully: {service.Name}");
                                }
                            }
                            catch (OperationCanceledException)
                            {
                                _logger.LogInfo($"Cleaning was cancelled during service: {service.Name}");
                                serviceResult.Success = false;
                                serviceResult.ErrorMessage = "Operation was cancelled";
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error in cleaning service {service.Name}: {ex.Message}");
                                serviceResult.Success = false;
                                serviceResult.ErrorMessage = ex.Message;
                                allSuccess = false;
                            }
        
                            return serviceResult;
                        }));
                    }
        
                    var serviceResults = await Task.WhenAll(tasks);
                    result.ServiceResults.AddRange(serviceResults);
        
                    result.TotalFilesProcessed = serviceResults.Sum(r => r.FilesProcessed);
                    result.TotalSpaceFreed = serviceResults.Sum(r => r.SpaceFreed);
                    result.EndTime = DateTime.Now;
                    result.Duration = result.EndTime - result.StartTime;
        
                    _logger.LogInfo($"Cleaning process completed. Overall success: {allSuccess}");
                    return result;
                }
    }
}