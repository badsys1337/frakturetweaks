using System;
using System.IO;
using System.Text.Json;

namespace Frakture_Tweaks.Services
{
    public class AppSettings
    {
        public string Language { get; set; } = "en-US";
    }

    public static class SettingsManager
    {
        private static readonly string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FraktureTweaks");
        private static readonly string SettingsFile = Path.Combine(SettingsFolder, "settings.json");
        
        public static AppSettings Settings { get; private set; } = new AppSettings();

        public static void Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                {
                    string json = File.ReadAllText(SettingsFile);
                    Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch
            {
                
                Settings = new AppSettings();
            }
        }

        public static void Save()
        {
            try
            {
                if (!Directory.Exists(SettingsFolder))
                {
                    Directory.CreateDirectory(SettingsFolder);
                }

                string json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFile, json);
            }
            catch
            {
                
            }
        }
    }
}
