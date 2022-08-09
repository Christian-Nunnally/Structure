﻿using System.IO;
using System.Text.Json;

namespace Structur.Program
{
    public class Settings
    {
        public static readonly string DefaultSettingsPath = "settings.structure";
        public static readonly string DefaultSavePath = "Save";

        public static Settings DefaultSettings
        {
            get
            {
                var settings = new Settings();
                settings.SavePath = Path.Combine(Directory.GetCurrentDirectory(), DefaultSavePath, "/");
                return settings;
            }
        }

        public static Settings ReadSettings(string settingsFilePath = null)
        {
            settingsFilePath ??= DefaultSettingsPath;
            if (!File.Exists(settingsFilePath)) WriteSettings(DefaultSettings, settingsFilePath);
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