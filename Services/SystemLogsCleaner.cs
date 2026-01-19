using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public class SystemLogsCleaner : ICleanerService
    {
        public string Name => "System Logs Cleaner";
        public string Description => "Removes system logs and event logs";

        private readonly IFileOperations _fileOperations;
                private readonly ILoggerService _logger;
                
                private long _spaceFreed = 0;
                private int _filesProcessed = 0;
        
                public long SpaceFreed => _spaceFreed;
                public int FilesProcessed => _filesProcessed;
        
                public SystemLogsCleaner(IFileOperations fileOperations, ILoggerService logger)
                {
                    _fileOperations = fileOperations;
                    _logger = logger;
                }
        
                public async Task<bool> CleanAsync(CancellationToken cancellationToken = default)
                {
                    _logger.LogInfo("Starting system logs cleaning process");
        
                    _spaceFreed = 0;
                    _filesProcessed = 0;
        
                    bool allSuccess = true;
        
                    try
                    {
                        await ClearLogFilesAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInfo("System logs cleaning was cancelled");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error clearing log files: {ex.Message}");
                        allSuccess = false;
                    }
        
                    try
                    {
                        await ClearEventLogsAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInfo("System logs cleaning was cancelled");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error clearing event logs: {ex.Message}");
                        allSuccess = false;
                    }
        
                    _logger.LogInfo($"Finished system logs cleaning process. Files processed: {_filesProcessed}, Space freed: {_spaceFreed} bytes");
                    return allSuccess;
                }
        
                public async Task<long> EstimateSizeAsync(CancellationToken cancellationToken = default)
                {
                    _logger.LogInfo("Estimating system logs size");
        
                    long totalSize = 0;
        
                    var systemDrive = Path.GetPathRoot(Environment.SystemDirectory);
        
                    string[] extensions = { "*.log", "*.old", "*.tmp", "*.bak", "*.trace" };
        
                    foreach (var extension in extensions)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
        
                        try
                        {
                            var size = await FileUtils.EstimateSizeAsync(systemDrive, extension, 10, cancellationToken);
                            totalSize += size;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            _logger.LogWarning($"Access denied when estimating size for {extension} files");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error estimating size for {extension} files: {ex.Message}");
                        }
                    }
        
                    var windowsDir = Environment.GetEnvironmentVariable("WINDIR");
                    var memoryDumpPath = Path.Combine(windowsDir, "memory.dmp");
                    
                    if (File.Exists(memoryDumpPath))
                    {
                        try
                        {
                            var fileInfo = new FileInfo(memoryDumpPath);
                            totalSize += fileInfo.Length;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error getting size of memory dump: {ex.Message}");
                        }
                    }
        
                    var miniDumpPath = Path.Combine(windowsDir, "Minidump");
                    if (Directory.Exists(miniDumpPath))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
        
                        try
                        {
                            var miniDumpSize = await FileUtils.EstimateSizeAsync(miniDumpPath, "*.dmp", 10, cancellationToken);
                            totalSize += miniDumpSize;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error estimating size of mini dumps: {ex.Message}");
                        }
                    }
        
                    _logger.LogInfo($"Estimated system logs size: {totalSize} bytes");
                    return totalSize;
                }

        private async Task ClearLogFilesAsync(CancellationToken cancellationToken)
        {
            var systemDrive = Path.GetPathRoot(Environment.SystemDirectory);

            // Удаление различных типов лог-файлов
            string[] extensions = { "*.log", "*.old", "*.tmp", "*.bak", "*.trace" };

            foreach (var extension in extensions)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Используем безопасный рекурсивный поиск с отслеживанием посещенных директорий
                    var files = FindFilesSafe(systemDrive, extension, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                    
                    foreach (var file in files)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!await _fileOperations.IsFileInUseAsync(file))
                        {
                            var fileInfo = new FileInfo(file);
                            bool success = await _fileOperations.MoveToRecycleBinAsync(file);
                            if (success)
                            {
                                _logger.LogInfo($"Log file moved to recycle bin: {file}");
                                _filesProcessed++;
                                _spaceFreed += fileInfo.Length;
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    _logger.LogWarning($"Access denied when searching for {extension} files");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error searching for {extension} files: {ex.Message}");
                }
            }

            // Удаление дампов памяти
            var windowsDir = Environment.GetEnvironmentVariable("WINDIR");
            var memoryDumpPath = Path.Combine(windowsDir, "memory.dmp");
            
            if (File.Exists(memoryDumpPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fileInfo = new FileInfo(memoryDumpPath);
                bool success = await _fileOperations.MoveToRecycleBinAsync(memoryDumpPath);
                if (success)
                {
                    _logger.LogInfo($"Memory dump file moved to recycle bin: {memoryDumpPath}");
                    _filesProcessed++;
                    _spaceFreed += fileInfo.Length;
                }
            }

            // Удаление мини-дампов
            var miniDumpPath = Path.Combine(windowsDir, "Minidump");
            if (Directory.Exists(miniDumpPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var miniDumpFiles = FindFilesSafe(miniDumpPath, "*.dmp", new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                
                foreach (var file in miniDumpFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!await _fileOperations.IsFileInUseAsync(file))
                    {
                        var fileInfo = new FileInfo(file);
                        bool success = await _fileOperations.MoveToRecycleBinAsync(file);
                        if (success)
                        {
                            _logger.LogInfo($"Mini dump file moved to recycle bin: {file}");
                            _filesProcessed++;
                            _spaceFreed += fileInfo.Length;
                        }
                    }
                }
            }
        }

        private List<string> FindFilesSafe(string rootPath, string searchPattern, HashSet<string> visitedDirectories)
        {
            var foundFiles = new List<string>();

            // Получаем нормализованный путь для предотвращения дубликатов из-за разных форм записи
            string normalizedPath = Path.GetFullPath(rootPath);

            // Проверяем, не посещали ли мы эту директорию ранее
            if (visitedDirectories.Contains(normalizedPath))
            {
                _logger.LogWarning($"Skipping already visited directory to prevent infinite loop: {rootPath}");
                return foundFiles;
            }

            // Добавляем текущую директорию в список посещенных
            visitedDirectories.Add(normalizedPath);

            try
            {
                // Находим файлы в текущей директории
                var files = Directory.GetFiles(rootPath, searchPattern);
                foundFiles.AddRange(files);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Access denied to directory {rootPath}: {ex.Message}");
                // Продолжаем с подкаталогами, если возможно
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error accessing directory {rootPath}: {ex.Message}");
                // Продолжаем с подкаталогами, если возможно
            }

            try
            {
                // Рекурсивно обрабатываем подкаталоги
                var subDirs = Directory.GetDirectories(rootPath);
                foreach (var subDir in subDirs)
                {
                    // Проверяем, не посещали ли мы эту поддиректорию ранее
                    string normalizedSubDirPath = Path.GetFullPath(subDir);
                    if (!visitedDirectories.Contains(normalizedSubDirPath))
                    {
                        var subDirFiles = FindFilesSafe(subDir, searchPattern, visitedDirectories);
                        foundFiles.AddRange(subDirFiles);
                    }
                    else
                    {
                        _logger.LogWarning($"Skipping already visited subdirectory to prevent infinite loop: {subDir}");
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning($"Access denied to subdirectories in {rootPath}: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error accessing subdirectories in {rootPath}: {ex.Message}");
            }

            // Удаляем директорию из множества посещенных при выходе из метода
            visitedDirectories.Remove(normalizedPath);

            return foundFiles;
        }

        private async Task ClearEventLogsAsync(CancellationToken cancellationToken)
        {
            _logger.LogInfo("Clearing Windows event logs...");

            try
            {
                // Попробуем использовать PowerShell для получения списка журналов и их очистки
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = "-Command \"Get-WinEvent -ListLog * | Where-Object {$_.RecordCount -gt 0} | ForEach-Object { Clear-WinEvent -LogName $_.LogName -Confirm:$false }\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        // Ждем завершения процесса с проверкой отмены
                        while (!process.HasExited)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            await Task.Delay(100);
                        }

                        var output = process.StandardOutput.ReadToEnd();
                        var error = process.StandardError.ReadToEnd();

                        if (!string.IsNullOrEmpty(output))
                        {
                            _logger.LogInfo($"Event logs clearing output: {output}");
                        }

                        if (!string.IsNullOrEmpty(error))
                        {
                            _logger.LogWarning($"Event logs clearing warnings: {error}");
                        }

                        _logger.LogInfo($"Event logs clearing completed with exit code: {process.ExitCode}");
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
                _logger.LogError($"Error clearing event logs with PowerShell: {ex.Message}");
                
                // Попробуем альтернативный метод через wevtutil
                await ClearEventLogsAlternativeAsync(cancellationToken);
            }
        }

        private async Task ClearEventLogsAlternativeAsync(CancellationToken cancellationToken)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "wevtutil",
                    Arguments = "el",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
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
                        var logNames = output.Split('\n');

                        foreach (var logName in logNames)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var trimmedLogName = logName.Trim();
                            if (!string.IsNullOrEmpty(trimmedLogName))
                            {
                                try
                                {
                                    await RunCommandAsync("wevtutil", $"cl \"{trimmedLogName}\"", cancellationToken);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Failed to clear event log {trimmedLogName}: {ex.Message}");
                                }
                            }
                        }
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
                _logger.LogError($"Error in alternative event logs clearing: {ex.Message}");
            }
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