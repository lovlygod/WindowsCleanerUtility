using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public class BrowserDataCleaner : ICleanerService
    {
        public string Name => "Browser Data Cleaner";
        public string Description => "Removes browser history and cookies";

        private readonly IFileOperations _fileOperations;
                private readonly ILoggerService _logger;
                
                private long _spaceFreed = 0;
                private int _filesProcessed = 0;
        
                public long SpaceFreed => _spaceFreed;
                public int FilesProcessed => _filesProcessed;
        
                public BrowserDataCleaner(IFileOperations fileOperations, ILoggerService logger)
                {
                    _fileOperations = fileOperations;
                    _logger = logger;
                }
        
                public async Task<bool> CleanAsync(CancellationToken cancellationToken = default)
                {
                    _logger.LogInfo("Starting browser data cleaning process");
        
                    _spaceFreed = 0;
                    _filesProcessed = 0;
        
                    bool allSuccess = true;
        
                    try
                    {
                        await ClearChromeDataAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInfo("Browser data cleaning was cancelled");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error clearing Chrome data: {ex.Message}");
                        allSuccess = false;
                    }
        
                    try
                    {
                        await ClearFirefoxDataAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInfo("Browser data cleaning was cancelled");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error clearing Firefox data: {ex.Message}");
                        allSuccess = false;
                    }
        
                    try
                    {
                        await ClearEdgeDataAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInfo("Browser data cleaning was cancelled");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error clearing Edge data: {ex.Message}");
                        allSuccess = false;
                    }
        
                    _logger.LogInfo($"Finished browser data cleaning process. Files processed: {_filesProcessed}, Space freed: {_spaceFreed} bytes");
                    return allSuccess;
                }
        
                public async Task<long> EstimateSizeAsync(CancellationToken cancellationToken = default)
                {
                    _logger.LogInfo("Estimating browser data size");
        
                    long totalSize = 0;
        
                    totalSize += await EstimateChromeDataSizeAsync(cancellationToken);
                    totalSize += await EstimateFirefoxDataSizeAsync(cancellationToken);
                    totalSize += await EstimateEdgeDataSizeAsync(cancellationToken);
        
                    _logger.LogInfo($"Estimated browser data size: {totalSize} bytes");
                    return totalSize;
                }

        private async Task<long> EstimateChromeDataSizeAsync(CancellationToken cancellationToken)
        {
            var chromePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google\\Chrome\\User Data\\Default");

            long totalSize = 0;

            if (Directory.Exists(chromePath))
            {
                // Оценка размера истории Chrome
                var historyPath = Path.Combine(chromePath, "History");
                if (File.Exists(historyPath))
                {
                    try
                    {
                        var fileInfo = new FileInfo(historyPath);
                        totalSize += fileInfo.Length;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error getting size of Chrome history: {ex.Message}");
                    }
                }

                // Оценка размера cookies Chrome
                var cookiesPath = Path.Combine(chromePath, "Cookies");
                if (File.Exists(cookiesPath))
                {
                    try
                    {
                        var fileInfo = new FileInfo(cookiesPath);
                        totalSize += fileInfo.Length;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error getting size of Chrome cookies: {ex.Message}");
                    }
                }
            }

            return totalSize;
        }

        private async Task<long> EstimateFirefoxDataSizeAsync(CancellationToken cancellationToken)
        {
            var firefoxPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Mozilla\\Firefox\\Profiles");

            long totalSize = 0;

            if (Directory.Exists(firefoxPath))
            {
                var profileDirs = Directory.GetDirectories(firefoxPath);
                
                foreach (var profileDir in profileDirs)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    // Оценка размера истории Firefox
                    var placesPath = Path.Combine(profileDir, "places.sqlite");
                    if (File.Exists(placesPath))
                    {
                        try
                        {
                            var fileInfo = new FileInfo(placesPath);
                            totalSize += fileInfo.Length;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error getting size of Firefox history: {ex.Message}");
                        }
                    }

                    // Оценка размера cookies Firefox
                    var cookiesPath = Path.Combine(profileDir, "cookies.sqlite");
                    if (File.Exists(cookiesPath))
                    {
                        try
                        {
                            var fileInfo = new FileInfo(cookiesPath);
                            totalSize += fileInfo.Length;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error getting size of Firefox cookies: {ex.Message}");
                        }
                    }
                }
            }

            return totalSize;
        }

        private async Task<long> EstimateEdgeDataSizeAsync(CancellationToken cancellationToken)
        {
            var edgePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft\\Edge\\User Data\\Default");

            long totalSize = 0;

            if (Directory.Exists(edgePath))
            {
                // Оценка размера истории Edge
                var historyPath = Path.Combine(edgePath, "History");
                if (File.Exists(historyPath))
                {
                    try
                    {
                        var fileInfo = new FileInfo(historyPath);
                        totalSize += fileInfo.Length;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error getting size of Edge history: {ex.Message}");
                    }
                }

                // Оценка размера cookies Edge
                var cookiesPath = Path.Combine(edgePath, "Cookies");
                if (File.Exists(cookiesPath))
                {
                    try
                    {
                        var fileInfo = new FileInfo(cookiesPath);
                        totalSize += fileInfo.Length;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error getting size of Edge cookies: {ex.Message}");
                    }
                }
            }

            return totalSize;
        }

        private async Task ClearChromeDataAsync(CancellationToken cancellationToken)
        {
            var chromePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google\\Chrome\\User Data\\Default");

            if (Directory.Exists(chromePath))
            {
                // Очистка истории Chrome
                var historyPath = Path.Combine(chromePath, "History");
                if (File.Exists(historyPath))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var fileInfo = new FileInfo(historyPath);
                    bool success = await _fileOperations.MoveToRecycleBinAsync(historyPath);
                    if (success)
                    {
                        _logger.LogInfo("Chrome history moved to recycle bin");
                        _filesProcessed++;
                        _spaceFreed += fileInfo.Length;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to move Chrome history to recycle bin");
                    }
                }

                // Очистка cookies Chrome
                var cookiesPath = Path.Combine(chromePath, "Cookies");
                if (File.Exists(cookiesPath))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var fileInfo = new FileInfo(cookiesPath);
                    bool success = await _fileOperations.MoveToRecycleBinAsync(cookiesPath);
                    if (success)
                    {
                        _logger.LogInfo("Chrome cookies moved to recycle bin");
                        _filesProcessed++;
                        _spaceFreed += fileInfo.Length;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to move Chrome cookies to recycle bin");
                    }
                }
            }
        }

        private async Task ClearFirefoxDataAsync(CancellationToken cancellationToken)
        {
            var firefoxPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Mozilla\\Firefox\\Profiles");

            if (Directory.Exists(firefoxPath))
            {
                var profileDirs = Directory.GetDirectories(firefoxPath);
                
                foreach (var profileDir in profileDirs)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    // Очистка истории Firefox
                    var placesPath = Path.Combine(profileDir, "places.sqlite");
                    if (File.Exists(placesPath))
                    {
                        var fileInfo = new FileInfo(placesPath);
                        bool success = await _fileOperations.MoveToRecycleBinAsync(placesPath);
                        if (success)
                        {
                            _logger.LogInfo("Firefox history moved to recycle bin");
                            _filesProcessed++;
                            _spaceFreed += fileInfo.Length;
                        }
                        else
                        {
                            _logger.LogWarning("Failed to move Firefox history to recycle bin");
                        }
                    }

                    // Очистка cookies Firefox
                    var cookiesPath = Path.Combine(profileDir, "cookies.sqlite");
                    if (File.Exists(cookiesPath))
                    {
                        var fileInfo = new FileInfo(cookiesPath);
                        bool success = await _fileOperations.MoveToRecycleBinAsync(cookiesPath);
                        if (success)
                        {
                            _logger.LogInfo("Firefox cookies moved to recycle bin");
                            _filesProcessed++;
                            _spaceFreed += fileInfo.Length;
                        }
                        else
                        {
                            _logger.LogWarning("Failed to move Firefox cookies to recycle bin");
                        }
                    }
                }
            }
        }

        private async Task ClearEdgeDataAsync(CancellationToken cancellationToken)
        {
            var edgePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft\\Edge\\User Data\\Default");

            if (Directory.Exists(edgePath))
            {
                // Очистка истории Edge
                var historyPath = Path.Combine(edgePath, "History");
                if (File.Exists(historyPath))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var fileInfo = new FileInfo(historyPath);
                    bool success = await _fileOperations.MoveToRecycleBinAsync(historyPath);
                    if (success)
                    {
                        _logger.LogInfo("Edge history moved to recycle bin");
                        _filesProcessed++;
                        _spaceFreed += fileInfo.Length;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to move Edge history to recycle bin");
                    }
                }

                // Очистка cookies Edge
                var cookiesPath = Path.Combine(edgePath, "Cookies");
                if (File.Exists(cookiesPath))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var fileInfo = new FileInfo(cookiesPath);
                    bool success = await _fileOperations.MoveToRecycleBinAsync(cookiesPath);
                    if (success)
                    {
                        _logger.LogInfo("Edge cookies moved to recycle bin");
                        _filesProcessed++;
                        _spaceFreed += fileInfo.Length;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to move Edge cookies to recycle bin");
                    }
                }
            }
        }
    }
}