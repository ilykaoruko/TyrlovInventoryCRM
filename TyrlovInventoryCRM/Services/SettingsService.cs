using System.IO;
using System.Text.Json;

namespace TyrlovInventoryCRM.Services
{
    public class AppSettings
    {
        public bool IsDarkTheme { get; set; }
    }

    public static class SettingsService
    {
        private const string FilePath = "settings.json";

        public static AppSettings LoadSettings()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null) return settings;
                }
            }
            catch { } 

            return new AppSettings { IsDarkTheme = false }; 
        }

        public static void SaveSettings(bool isDarkTheme)
        {
            try
            {
                var settings = new AppSettings { IsDarkTheme = isDarkTheme };
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch { }
        }
    }
}