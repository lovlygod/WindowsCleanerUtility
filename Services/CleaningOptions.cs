using System;

namespace WindowsCleanerUtility.Services
{
    public class CleaningOptions
    {
        public bool IncludeTemporaryFiles { get; set; } = true;
        public bool IncludeLogFiles { get; set; } = true;
        public bool IncludeEventLogs { get; set; } = true;
        public bool IncludeOldFiles { get; set; } = true;
        public bool IncludeBrowserHistory { get; set; } = true;
        public bool IncludeBrowserCookies { get; set; } = true;
        public bool IncludeDNSTempFiles { get; set; } = true;
        
        // Опции для продвинутой очистки
        public int DaysForOldFiles { get; set; } = 30;
        public bool MoveToRecycleBin { get; set; } = true;
        public bool ShowProgress { get; set; } = true;
    }
}