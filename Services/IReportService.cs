using System.Threading.Tasks;

namespace WindowsCleanerUtility.Services
{
    public interface IReportService
    {
        Task<string> GenerateReportAsync(CleaningResult result, ReportFormat format);
    }
    
    public enum ReportFormat
    {
        JSON,
        XML,
        CSV,
        HTML
    }
    
    public class CleaningResult
    {
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
        public System.TimeSpan Duration { get; set; }
        public System.Collections.Generic.List<ServiceResult> ServiceResults { get; set; } = new System.Collections.Generic.List<ServiceResult>();
        public long TotalFilesProcessed { get; set; }
        public long TotalSpaceFreed { get; set; }
    }
    
    public class ServiceResult
    {
        public string ServiceName { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public long FilesProcessed { get; set; }
        public long SpaceFreed { get; set; }
        public System.DateTime StartTime { get; set; }
        public System.DateTime EndTime { get; set; }
    }
}