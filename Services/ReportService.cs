using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace WindowsCleanerUtility.Services
{
    public class ReportService : IReportService
    {
        private readonly ILoggerService _logger;
        
        public ReportService(ILoggerService logger)
        {
            _logger = logger;
        }
        
        public async Task<string> GenerateReportAsync(CleaningResult result, ReportFormat format)
        {
            try
            {
                string reportContent;
                
                switch (format)
                {
                    case ReportFormat.JSON:
                        reportContent = GenerateJsonReport(result);
                        break;
                    case ReportFormat.XML:
                        reportContent = GenerateXmlReport(result);
                        break;
                    case ReportFormat.CSV:
                        reportContent = GenerateCsvReport(result);
                        break;
                    case ReportFormat.HTML:
                        reportContent = GenerateHtmlReport(result);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported report format: {format}");
                }
                
                _logger.LogInfo($"Generated {format} report with {result.TotalFilesProcessed} files processed");
                
                return reportContent;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating report: {ex.Message}");
                throw;
            }
        }
        
        private string GenerateJsonReport(CleaningResult result)
        {
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
        
        private string GenerateXmlReport(CleaningResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<CleaningReport>");
            sb.AppendLine($"  <StartTime>{result.StartTime:yyyy-MM-ddTHH:mm:ss}</StartTime>");
            sb.AppendLine($"  <EndTime>{result.EndTime:yyyy-MM-ddTHH:mm:ss}</EndTime>");
            sb.AppendLine($"  <Duration>{result.Duration}</Duration>");
            sb.AppendLine($"  <TotalFilesProcessed>{result.TotalFilesProcessed}</TotalFilesProcessed>");
            sb.AppendLine($"  <TotalSpaceFreed>{result.TotalSpaceFreed}</TotalSpaceFreed>");
            sb.AppendLine("  <ServiceResults>");
            
            foreach (var serviceResult in result.ServiceResults)
            {
                sb.AppendLine("    <ServiceResult>");
                sb.AppendLine($"      <ServiceName>{serviceResult.ServiceName}</ServiceName>");
                sb.AppendLine($"      <Success>{serviceResult.Success}</Success>");
                sb.AppendLine($"      <ErrorMessage>{serviceResult.ErrorMessage}</ErrorMessage>");
                sb.AppendLine($"      <FilesProcessed>{serviceResult.FilesProcessed}</FilesProcessed>");
                sb.AppendLine($"      <SpaceFreed>{serviceResult.SpaceFreed}</SpaceFreed>");
                sb.AppendLine($"      <StartTime>{serviceResult.StartTime:yyyy-MM-ddTHH:mm:ss}</StartTime>");
                sb.AppendLine($"      <EndTime>{serviceResult.EndTime:yyyy-MM-ddTHH:mm:ss}</EndTime>");
                sb.AppendLine("    </ServiceResult>");
            }
            
            sb.AppendLine("  </ServiceResults>");
            sb.AppendLine("</CleaningReport>");
            
            return sb.ToString();
        }
        
        private string GenerateCsvReport(CleaningResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Service Name,Success,Error Message,Files Processed,Space Freed (bytes),Start Time,End Time");
            
            foreach (var serviceResult in result.ServiceResults)
            {
                sb.AppendLine($"\"{serviceResult.ServiceName}\",{serviceResult.Success},\"{serviceResult.ErrorMessage}\",{serviceResult.FilesProcessed},{serviceResult.SpaceFreed},\"{serviceResult.StartTime:yyyy-MM-dd HH:mm:ss}\",\"{serviceResult.EndTime:yyyy-MM-dd HH:mm:ss}\"");
            }
            
            return sb.ToString();
        }
        
        private string GenerateHtmlReport(CleaningResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("  <title>Cleaning Report</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine("    body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine("    table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("    th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            sb.AppendLine("    th { background-color: #f2f2f2; }");
            sb.AppendLine("    .success { color: green; }");
            sb.AppendLine("    .error { color: red; }");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("  <h1>Cleaning Report</h1>");
            sb.AppendLine($"  <p><strong>Total Files Processed:</strong> {result.TotalFilesProcessed}</p>");
            sb.AppendLine($"  <p><strong>Total Space Freed:</strong> {FormatBytes(result.TotalSpaceFreed)}</p>");
            sb.AppendLine($"  <p><strong>Duration:</strong> {result.Duration}</p>");
            sb.AppendLine("  <table>");
            sb.AppendLine("    <tr><th>Service Name</th><th>Status</th><th>Error Message</th><th>Files Processed</th><th>Space Freed</th><th>Start Time</th><th>End Time</th></tr>");
            
            foreach (var serviceResult in result.ServiceResults)
            {
                var statusClass = serviceResult.Success ? "success" : "error";
                var statusText = serviceResult.Success ? "Success" : "Failed";
                
                sb.AppendLine($"    <tr>");
                sb.AppendLine($"      <td>{serviceResult.ServiceName}</td>");
                sb.AppendLine($"      <td class=\"{statusClass}\">{statusText}</td>");
                sb.AppendLine($"      <td>{serviceResult.ErrorMessage ?? ""}</td>");
                sb.AppendLine($"      <td>{serviceResult.FilesProcessed}</td>");
                sb.AppendLine($"      <td>{FormatBytes(serviceResult.SpaceFreed)}</td>");
                sb.AppendLine($"      <td>{serviceResult.StartTime:yyyy-MM-dd HH:mm:ss}</td>");
                sb.AppendLine($"      <td>{serviceResult.EndTime:yyyy-MM-dd HH:mm:ss}</td>");
                sb.AppendLine($"    </tr>");
            }
            
            sb.AppendLine("  </table>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            
            return sb.ToString();
        }
        
        private string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
    }
}