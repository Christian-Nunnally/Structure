using Structure.Program;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Structure.IO.Persistence
{
    public static class StructureFileIO
    {
        public const string SaveFileExtension = ".structure";
        public const string AppDataSettingsFolderName = "Structure";
        private static readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();
        private static string savePath;

        public static string SavePath => savePath ?? (savePath = GetSavedPath());

        public static string ReadFromFile(string key) => _cache.TryGetValue(key, out var value) ? value
            : (_cache[key] = File.Exists(GetFileName(key)) ? File.ReadAllText(GetFileName(key)) : string.Empty);

        public static bool DoesFileExist(string key) => _cache.ContainsKey(key) || File.Exists(GetFileName(key));

        public static void Set(string key, string value)
        {
            if (ReadFromFile(key) != value) File.WriteAllText(GetFileName(key), value);
            _cache[key] = value;
        }

        private static string GetSavedPath()
        {
            var settings = Settings.ReadSettings();
            if (settings.SavePath == null) throw new InvalidOperationException($"Supply a save path in the {Settings.DefaultSettingsPath} file.");
           return settings.SavePath;
        }

        private static string GetFileName(string key) => $"{SavePath}{key}{SaveFileExtension}";
    }
}