using System;
using System.IO;
using System.Text.Json;

namespace WindowsCleanerUtility.Settings
{
    public class UserSettings
    {
        public bool IncludeTemporaryFiles { get; set; } = true;
        public bool IncludeLogFiles { get; set; } = true;
        public bool IncludeEventLogs { get; set; } = true;
        public bool IncludeOldFiles { get; set; } = true;
        public bool IncludeBrowserHistory { get; set; } = true;
        public bool IncludeBrowserCookies { get; set; } = true;
        public bool IncludeDNSTempFiles { get; set; } = true;
        public int DaysForOldFiles { get; set; } = 30;
        public bool MoveToRecycleBin { get; set; } = true;
        public bool ShowProgress { get; set; } = true;
        public string Theme { get; set; } = "Dark"; // "Dark", "Light", "System"
        public string Language { get; set; } = "ru-RU"; // Языковая локализация

        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WindowsCleanerUtility",
            "settings.json");

        public static UserSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
                }
            }
            catch (Exception)
            {
                // Если произошла ошибка при чтении настроек, возвращаем настройки по умолчанию
            }

            return new UserSettings();
        }

        public void Save()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception)
            {
                // Если не удается сохранить настройки, игнорируем ошибку
            }
        }
    }
}