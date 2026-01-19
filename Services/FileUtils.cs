using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public static class FileUtils
    {
        /// <summary>
        /// Безопасно перемещает файл в корзину
        /// </summary>
        /// <param name="filePath">Путь к файлу</param>
        /// <param name="fileOps">Сервис файловых операций</param>
        /// <param name="logger">Сервис логирования</param>
        /// <returns>True если операция успешна, иначе false</returns>
        public static async Task<bool> SafeMoveToRecycleBinAsync(
            string filePath, 
            IFileOperations fileOps, 
            ILoggerService logger)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;
                    
                if (await fileOps.IsFileInUseAsync(filePath))
                {
                    logger.LogWarning($"File is in use: {filePath}");
                    return false;
                }
                
                return await fileOps.MoveToRecycleBinAsync(filePath);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error processing file {filePath}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Асинхронно перечисляет файлы в директории с заданным паттерном
        /// </summary>
        /// <param name="rootPath">Корневая директория для поиска</param>
        /// <param name="searchPattern">Паттерн поиска файлов</param>
        /// <param name="maxDepth">Максимальная глубина рекурсии</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Асинхронный поток файлов</returns>
        public static async IAsyncEnumerable<string> EnumerateFilesAsync(
            string rootPath, 
            string searchPattern, 
            int maxDepth = 10,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            });
            
            var writer = channel.Writer;
            var reader = channel.Reader;
            
            // Запускаем задачу для заполнения канала
            var enumerationTask = Task.Run(async () =>
            {
                try
                {
                    await EnumerateFilesInternalAsync(rootPath, searchPattern, maxDepth, 0, writer, cancellationToken);
                    writer.Complete();
                }
                catch (Exception ex)
                {
                    writer.Complete(ex);
                }
            }, cancellationToken);
            
            // Читаем файлы из канала
            await foreach (var file in reader.ReadAllAsync(cancellationToken))
            {
                yield return file;
            }
            
            await enumerationTask;
        }
        
        private static async Task EnumerateFilesInternalAsync(
            string currentPath, 
            string searchPattern, 
            int maxDepth, 
            int currentDepth,
            ChannelWriter<string> writer,
            CancellationToken cancellationToken)
        {
            if (currentDepth > maxDepth)
                return;
                
            cancellationToken.ThrowIfCancellationRequested();
            
            string[] files = null;
            try
            {
                files = Directory.GetFiles(currentPath, searchPattern);
            }
            catch (UnauthorizedAccessException)
            {
                return; // Пропустить директорию без доступа
            }
            catch (Exception)
            {
                return; // Пропустить проблемную директорию
            }
            
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await writer.WriteAsync(file, cancellationToken);
            }
            
            try
            {
                var subDirs = Directory.GetDirectories(currentPath);
                foreach (var subDir in subDirs)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await EnumerateFilesInternalAsync(subDir, searchPattern, maxDepth, currentDepth + 1, writer, cancellationToken);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Пропустить поддиректории без доступа
            }
            catch (Exception)
            {
                // Пропустить проблемные поддиректории
            }
        }
        
        /// <summary>
        /// Оценивает размер файлов, соответствующих паттерну
        /// </summary>
        /// <param name="rootPath">Корневая директория для поиска</param>
        /// <param name="searchPattern">Паттерн поиска файлов</param>
        /// <param name="maxDepth">Максимальная глубина рекурсии</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Общий размер файлов в байтах</returns>
        public static async Task<long> EstimateSizeAsync(
            string rootPath, 
            string searchPattern, 
            int maxDepth = 10,
            CancellationToken cancellationToken = default)
        {
            long totalSize = 0;
            
            await foreach (var file in EnumerateFilesAsync(rootPath, searchPattern, maxDepth, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                try
                {
                    var fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }
                catch
                {
                    // Пропустить файлы с ошибками доступа
                }
            }
            
            return totalSize;
        }
    }
}