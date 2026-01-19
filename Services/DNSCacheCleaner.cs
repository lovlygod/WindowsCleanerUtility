using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public class DNSCacheCleaner : ICleanerService
    {
        public string Name => "DNS Cache Cleaner";
        public string Description => "Flushes DNS resolver cache";

        private readonly ILoggerService _logger;
                
                private long _spaceFreed = 0;
                private int _filesProcessed = 0;
        
                public long SpaceFreed => _spaceFreed;
                public int FilesProcessed => _filesProcessed;
        
                public DNSCacheCleaner(ILoggerService logger)
                {
                    _logger = logger;
                }
        
                public async Task<bool> CleanAsync(CancellationToken cancellationToken = default)
                {
                    _logger.LogInfo("Starting DNS cache cleaning process");
        
                    _spaceFreed = 0;
                    _filesProcessed = 0;
        
                    try
                    {
                        await RunCommandAsync("ipconfig", "/flushdns", cancellationToken);
                        _logger.LogInfo("DNS cache cleared successfully");
                        _spaceFreed = 0;
                        _filesProcessed = 0;
                        return true;
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInfo("DNS cache cleaning was cancelled");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error clearing DNS cache: {ex.Message}");
                        return false;
                    }
                }
        
                public async Task<long> EstimateSizeAsync(CancellationToken cancellationToken = default)
                {
                    _logger.LogInfo("Estimating DNS cache size");
                    return 0;
                }

        private async Task RunCommandAsync(string fileName, string arguments, CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process != null)
                {
                    while (!process.HasExited)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await Task.Delay(100);
                    }

                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(output))
                    {
                        _logger.LogInfo($"Command {fileName} {arguments} output: {output}");
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        _logger.LogWarning($"Command {fileName} {arguments} error: {error}");
                    }

                    _logger.LogInfo($"Command {fileName} {arguments} completed with exit code: {process.ExitCode}");
                }
            }
        }
    }
}