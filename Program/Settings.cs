using System.IO;
using System.Text.Json;

namespace Structur.Program
{
    internal class Settings
    {
        public static string DefaultSettingsPath = "settings.structure";

        public static Settings ReadSettings(string settingsFilePath = null)
        {
            settingsFilePath ??= DefaultSettingsPath;
            if (!File.Exists(settingsFilePath)) WriteSettings(new Settings(), settingsFilePath);
            return JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsFilePath));
        }

        public static void WriteSettings(Settings settings, string settingsFilePath = null)
        {
            settingsFilePath ??= DefaultSettingsPath;
            var options = new JsonSerializerOptions();
            options.WriteIndented = true;
            File.WriteAllText(settingsFilePath, JsonSerializer.Serialize(settings, options));
        }

        public bool EnableWebServer { get; set; }

        public bool EnableClient { get; set; }

        public string Hostname { get; set; }

        public string ServerHostname { get; set; }

        public bool EnableDebugging { get; set; }

        public string SavePath { get; set; }
    }
}