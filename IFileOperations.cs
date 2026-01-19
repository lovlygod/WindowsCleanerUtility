using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public interface IFileOperations
    {
        Task<bool> MoveToRecycleBinAsync(string filePath);
        Task<bool> DeletePermanentlyAsync(string filePath);
        Task<bool> IsFileInUseAsync(string filePath);
    }
}