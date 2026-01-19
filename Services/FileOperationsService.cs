using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace WindowsCleanerUtility.Services
{
    public class FileOperationsService : IFileOperations
    {
        private readonly ILoggerService _logger;

        public FileOperationsService(ILoggerService logger)
        {
            _logger = logger;
        }

        public async Task<bool> MoveToRecycleBinAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                // Проверяем, используется ли файл другим процессом
                if (await IsFileInUseAsync(filePath))
                {
                    _logger.LogWarning($"Cannot move file to recycle bin, file is in use: {filePath}");
                    return false;
                }

                await Task.Run(() =>
                {
                    FileSystem.DeleteFile(filePath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                });
                
                _logger.LogInfo($"File moved to recycle bin: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to move file to recycle bin {filePath}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeletePermanentlyAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                // Проверяем, используется ли файл другим процессом
                if (await IsFileInUseAsync(filePath))
                {
                    _logger.LogWarning($"Cannot permanently delete file, file is in use: {filePath}");
                    return false;
                }

                await Task.Run(() =>
                {
                    File.Delete(filePath);
                });
                
                _logger.LogInfo($"File permanently deleted: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to permanently delete file {filePath}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> IsFileInUseAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        stream.Close();
                        return false;
                    }
                }
                catch (IOException)
                {
                    // Файл занят другим процессом
                    return true;
                }
            });
        }
    }
}