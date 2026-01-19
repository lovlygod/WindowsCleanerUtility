using System.Threading;
using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public interface ICleanerService
        {
            string Name { get; }
            
            string Description { get; }
            
            Task<bool> CleanAsync(CancellationToken cancellationToken = default);
            
            Task<long> EstimateSizeAsync(CancellationToken cancellationToken = default);
            
            int FilesProcessed { get; }
            
            long SpaceFreed { get; }
        }
}