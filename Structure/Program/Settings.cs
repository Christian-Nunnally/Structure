using System.IO;
using System.Text.Json;

namespace Structur.Program
{
    public class Settings
    {
        public static readonly string DefaultSettingsFile = "settings.structure";
        public static readonly string DefaultSaveDirectory = "Save";

        public static Settings DefaultSettings
        {
            get
            {
                var settings = new Settings();
                settings.SavePath = $"{Directory.GetCurrentDirectory()}\\{DefaultSaveDirectory}\\";
                Directory.CreateDirectory(settings.SavePath);
                return settings;
            }
        }

        public static Settings ReadSettings()
        {
            if (!File.Exists(DefaultSettingsFile)) WriteSettings(DefaultSettings);
            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(DefaultSettingsFile));
        }

        public static void WriteSettings(Settings settings)
        {
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            File.WriteAllText(DefaultSettingsFile, JsonSerializer.Serialize(settings, options));
        }

        public bool EnableWebServer { get; set; }

        public bool EnableClient { get; set; }

        public string Hostname { get; set; }

        public string ServerHostname { get; set; }

        public bool EnableDebugging { get; set; }

        public string SavePath { get; set; }
    }
}