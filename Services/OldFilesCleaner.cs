using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public class OldFilesCleaner : ICleanerService
    {
        public string Name => "Old Files Cleaner";
        public string Description => "Removes old files based on creation date";

        private readonly IFileOperations _fileOperations;
                private readonly ILoggerService _logger;
                private readonly int _daysThreshold;
                
                private long _spaceFreed = 0;
                private int _filesProcessed = 0;
        
                public long SpaceFreed => _spaceFreed;
                public int FilesProcessed => _filesProcessed;
        
                public OldFilesCleaner(IFileOperations fileOperations, ILoggerService logger, int daysThreshold = 30)
                {
                    _fileOperations = fileOperations;
                    _logger = logger;
                    _daysThreshold = daysThreshold;
                }
        
                public async Task<bool> CleanAsync(CancellationToken cancellationToken = default)
                {
                    _logger.LogInfo($"Starting old files cleaning process (files older than {_daysThreshold} days)");
        
                    _spaceFreed = 0;
                    _filesProcessed = 0;
        
                    var cutoffDate = DateTime.Now.AddDays(-_daysThreshold);
        
                    var tempPaths = new[]
                    {
                        Path.GetTempPath(),
                        Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "", "Temp"),
                        Path.Combine(Environment.SystemDirectory, "temp")
                    };
        
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
                            await ProcessDirectoryForOldFilesAsync(tempPath, cutoffDate, cancellationToken, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInfo("Old files cleaning was cancelled");
                            return false;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing directory {tempPath}: {ex.Message}");
                            allSuccess = false;
                        }
                    }
        
                    _logger.LogInfo($"Finished old files cleaning process. Files processed: {_filesProcessed}, Space freed: {_spaceFreed} bytes");
                    return allSuccess;
                }
        
                public async Task<long> EstimateSizeAsync(CancellationToken cancellationToken = default)
                {
                    _logger.LogInfo($"Estimating old files size (files older than {_daysThreshold} days)");
        
                    var cutoffDate = DateTime.Now.AddDays(-_daysThreshold);
        
                    var tempPaths = new[]
                    {
                        Path.GetTempPath(),
                        Path.Combine(Environment.GetEnvironmentVariable("LOCALAPPDATA") ?? "", "Temp"),
                        Path.Combine(Environment.SystemDirectory, "temp")
                    };
        
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
                            var directorySize = await EstimateOldFilesSizeAsync(tempPath, cutoffDate, cancellationToken);
                            totalSize += directorySize;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error estimating size for directory {tempPath}: {ex.Message}");
                        }
                    }
        
                    _logger.LogInfo($"Estimated old files size: {totalSize} bytes");
                    return totalSize;
                }

        private async Task<long> EstimateOldFilesSizeAsync(string directoryPath, DateTime cutoffDate, CancellationToken cancellationToken, HashSet<string>? visitedDirectories = null)
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
                return 0;
            }

            // Добавляем текущую директорию в список посещенных
            visitedDirectories.Add(normalizedPath);

            long totalSize = 0;

            var dirInfo = new DirectoryInfo(directoryPath);
            
            // Проверяем, что директория все еще существует
            if (!dirInfo.Exists)
            {
                return 0;
            }

            FileInfo[] files = null;
            try
            {
                files = dirInfo.GetFiles();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Access denied to directory {directoryPath}: {ex.Message}");
                return 0; // Пропускаем директорию, если нет доступа
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error accessing directory {directoryPath}: {ex.Message}");
                return 0; // Пропускаем директорию при любой другой ошибке
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Проверяем, старше ли файл заданного порога
                    if (file.CreationTime < cutoffDate || file.LastWriteTime < cutoffDate)
                    {
                        totalSize += file.Length;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error checking old file {file.FullName}: {ex.Message}");
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
                return totalSize; // Пропускаем подкаталоги, если нет доступа
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error accessing subdirectories in {directoryPath}: {ex.Message}");
                return totalSize; // Пропускаем подкаталоги при любой другой ошибке
            }

            foreach (var subDir in subDirs)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                // Проверяем, не посещали ли мы эту поддиректорию ранее
                string normalizedSubDirPath = Path.GetFullPath(subDir.FullName);
                if (!visitedDirectories.Contains(normalizedSubDirPath))
                {
                    var subDirSize = await EstimateOldFilesSizeAsync(subDir.FullName, cutoffDate, cancellationToken, visitedDirectories);
                    totalSize += subDirSize;
                }
            }

            // Удаляем директорию из множества посещенных при выходе из метода
            // Это позволяет корректно обрабатывать случаи, когда одна и та же директория
            // может быть достигнута разными путями
            visitedDirectories.Remove(normalizedPath);

            return totalSize;
        }

        private async Task ProcessDirectoryForOldFilesAsync(string directoryPath, DateTime cutoffDate, CancellationToken cancellationToken, HashSet<string>? visitedDirectories = null)
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

            _logger.LogInfo($"Processing directory for old files: {directoryPath}");

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
                    // Проверяем, старше ли файл заданного порога
                    if (file.CreationTime < cutoffDate || file.LastWriteTime < cutoffDate)
                    {
                        if (!await _fileOperations.IsFileInUseAsync(file.FullName))
                        {
                            bool success = await _fileOperations.MoveToRecycleBinAsync(file.FullName);
                            if (success)
                            {
                                _logger.LogInfo($"Old file moved to recycle bin: {file.FullName} (Created: {file.CreationTime})");
                                // Увеличиваем счетчики только при успешном перемещении
                                _filesProcessed++;
                                _spaceFreed += file.Length;
                            }
                            else
                            {
                                _logger.LogWarning($"Failed to move old file to recycle bin: {file.FullName}");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"Old file is in use and cannot be moved: {file.FullName}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Не перебрасываем исключение, чтобы не отменять весь процесс
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing old file {file.FullName}: {ex.Message}");
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
                    await ProcessDirectoryForOldFilesAsync(subDir.FullName, cutoffDate, cancellationToken, visitedDirectories);
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
    }
}