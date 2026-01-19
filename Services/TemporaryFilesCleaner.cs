using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public class TemporaryFilesCleaner : ICleanerService
    {
        public string Name => "Temporary Files Cleaner";
        public string Description => "Removes temporary files from various system locations";

        private readonly IFileOperations _fileOperations;
                private readonly ILoggerService _logger;
        
                public TemporaryFilesCleaner(IFileOperations fileOperations, ILoggerService logger)
                {
                    _fileOperations = fileOperations;
                    _logger = logger;
                }
        
                private long _spaceFreed = 0;
                private int _filesProcessed = 0;
        
                public long SpaceFreed => _spaceFreed;
                public int FilesProcessed => _filesProcessed;
        
                public async Task<bool> CleanAsync(CancellationToken cancellationToken = default)
                {
                    _logger.LogInfo("Starting temporary files cleaning process");
        
                    _spaceFreed = 0;
                    _filesProcessed = 0;
        
                    var tempPaths = new List<string>
                    {
                        Path.GetTempPath(),
                        Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "", "Temp"),
                        Path.Combine(Environment.SystemDirectory, "temp")
                    };
        
                    var prefetchPath = Path.Combine(Environment.SystemDirectory, "Prefetch");
                    if (Directory.Exists(prefetchPath))
                    {
                        tempPaths.Add(prefetchPath);
                    }
        
                    bool allSuccess = true;
        
                    foreach (var tempPath in tempPaths)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
        
                        if (!Directory.Exists(tempPath))
                        {
                            _logger.LogWarning($"Directory does not exist, skipping: {tempPath}");
                            continue;
                        }
        
                        try
                        {
                            await ProcessDirectoryAsync(tempPath, cancellationToken, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInfo("Temporary files cleaning was cancelled");
                            return false;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing directory {tempPath}: {ex.Message}");
                            allSuccess = false;
                        }
                    }
        
                    await ClearExplorerCacheAsync(cancellationToken);
        
                    _logger.LogInfo($"Finished temporary files cleaning process. Files processed: {_filesProcessed}, Space freed: {_spaceFreed} bytes");
                    return allSuccess;
                }
        
                public async Task<long> EstimateSizeAsync(CancellationToken cancellationToken = default)
                {
                    _logger.LogInfo("Estimating temporary files size");
        
                    var tempPaths = new List<string>
                    {
                        Path.GetTempPath(),
                        Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "", "Temp"),
                        Path.Combine(Environment.SystemDirectory, "temp")
                    };
        
                    var prefetchPath = Path.Combine(Environment.SystemDirectory, "Prefetch");
                    if (Directory.Exists(prefetchPath))
                    {
                        tempPaths.Add(prefetchPath);
                    }
        
                    long totalSize = 0;
        
                    foreach (var tempPath in tempPaths)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
        
                        if (!Directory.Exists(tempPath))
                        {
                            continue;
                        }
        
                        try
                        {
                            var directorySize = await FileUtils.EstimateSizeAsync(tempPath, "*", 10, cancellationToken);
                            totalSize += directorySize;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error estimating size for directory {tempPath}: {ex.Message}");
                        }
                    }
        
                    totalSize += await EstimateExplorerCacheSizeAsync(cancellationToken);
        
                    _logger.LogInfo($"Estimated temporary files size: {totalSize} bytes");
                    return totalSize;
                }

        private async Task ProcessDirectoryAsync(string directoryPath, CancellationToken cancellationToken, HashSet<string>? visitedDirectories = null)
        {
            // Инициализируем множество посещенных директорий, если оно не передано
            if (visitedDirectories == null)
            {
                visitedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            // Получаем нормализованный путь для предотвращения дубликатов из-за разных форм записи
            string normalizedPath = Path.GetFullPath(directoryPath);

            // Проверяем, не посещали ли мы эту директорию ранее
            if (visitedDirectories.Contains(normalizedPath))
            {
                _logger.LogWarning($"Skipping already visited directory to prevent infinite loop: {directoryPath}");
                return;
            }

            // Добавляем текущую директорию в список посещенных
            visitedDirectories.Add(normalizedPath);

            _logger.LogInfo($"Processing directory: {directoryPath}");

            var dirInfo = new DirectoryInfo(directoryPath);
            
            // Проверяем, что директория все еще существует
            if (!dirInfo.Exists)
            {
                _logger.LogWarning($"Directory no longer exists: {directoryPath}");
                return;
            }

            FileInfo[] files = null;
            try
            {
                files = dirInfo.GetFiles();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Access denied to directory {directoryPath}: {ex.Message}");
                return; // Пропускаем директорию, если нет доступа
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error accessing directory {directoryPath}: {ex.Message}");
                return; // Пропускаем директорию при любой другой ошибке
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (!await _fileOperations.IsFileInUseAsync(file.FullName))
                    {
                        // Сохраняем размер файла до его перемещения
                        var fileSize = file.Length;
                        
                        bool success = await _fileOperations.MoveToRecycleBinAsync(file.FullName);
                        if (success)
                        {
                            _logger.LogInfo($"File moved to recycle bin: {file.FullName}");
                            // Увеличиваем счетчики только при успешном перемещении
                            _filesProcessed++;
                            _spaceFreed += fileSize;
                        }
                        else
                        {
                            _logger.LogWarning($"Failed to move file to recycle bin: {file.FullName}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"File is in use and cannot be moved: {file.FullName}");
                    }
                }
                catch (OperationCanceledException)
                {
                    // Не перебрасываем исключение, чтобы не отменять весь процесс
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error moving file to recycle bin {file.FullName}: {ex.Message}");
                }
            }

            // Рекурсивно обрабатываем подкаталоги
            DirectoryInfo[] subDirs = null;
            try
            {
                subDirs = dirInfo.GetDirectories();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Access denied to subdirectories in {directoryPath}: {ex.Message}");
                return; // Пропускаем подкаталоги, если нет доступа
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error accessing subdirectories in {directoryPath}: {ex.Message}");
                return; // Пропускаем подкаталоги при любой другой ошибке
            }

            foreach (var subDir in subDirs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                // Проверяем, не посещали ли мы эту поддиректорию ранее
                string normalizedSubDirPath = Path.GetFullPath(subDir.FullName);
                if (!visitedDirectories.Contains(normalizedSubDirPath))
                {
                    await ProcessDirectoryAsync(subDir.FullName, cancellationToken, visitedDirectories);
                }
                else
                {
                    _logger.LogWarning($"Skipping already visited subdirectory to prevent infinite loop: {subDir.FullName}");
                }
            }

            // Удаляем директорию из множества посещенных при выходе из метода
            // Это позволяет корректно обрабатывать случаи, когда одна и та же директория
            // может быть достигнута разными путями
            visitedDirectories.Remove(normalizedPath);
        }

        private async Task<long> EstimateExplorerCacheSizeAsync(CancellationToken cancellationToken)
        {
            var explorerCachePath = Path.Combine(
                Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "",
                "Microsoft", "Windows", "Explorer");

            if (Directory.Exists(explorerCachePath))
            {
                var thumbCacheFiles = Directory.GetFiles(explorerCachePath, "thumbcache_*.db", SearchOption.TopDirectoryOnly);
                long totalSize = 0;

                foreach (var file in thumbCacheFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var fileInfo = new FileInfo(file);
                        totalSize += fileInfo.Length;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error getting size of Explorer cache file {file}: {ex.Message}");
                    }
                }

                return totalSize;
            }

            return 0;
        }

        private async Task ClearExplorerCacheAsync(CancellationToken cancellationToken)
        {
            var explorerCachePath = Path.Combine(
                Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "",
                "Microsoft", "Windows", "Explorer");

            if (Directory.Exists(explorerCachePath))
            {
                _logger.LogInfo($"Clearing Explorer cache: {explorerCachePath}");

                var thumbCacheFiles = Directory.GetFiles(explorerCachePath, "thumbcache_*.db", SearchOption.TopDirectoryOnly);

                foreach (var file in thumbCacheFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var fileInfo = new FileInfo(file);
                        bool success = await _fileOperations.MoveToRecycleBinAsync(file);
                        if (success)
                        {
                            _logger.LogInfo($"Explorer cache file moved to recycle bin: {file}");
                            _filesProcessed++;
                            _spaceFreed += fileInfo.Length;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Не перебрасываем исключение, чтобы не отменять весь процесс
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error moving Explorer cache file to recycle bin {file}: {ex.Message}");
                    }
                }
            }
        }
    }
}