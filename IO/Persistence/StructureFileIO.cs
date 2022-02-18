using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Structure.IO.Persistence
{
    public static class StructureFileIO
    {
        public const string SaveFileExtension = ".structure";
        public const string AppDataSettingsFolderName = "Structure";
        private static readonly ConcurrentDictionary<string, string> _cache = new ConcurrentDictionary<string, string>();
        private static string savePath;

        public static string SavePath => savePath ?? (savePath = GetSavedDirectoryPath(nameof(SavePath)));

        public static string ReadFromFile(string key) => _cache.TryGetValue(key, out var value) ? value
            : (_cache[key] = File.Exists(GetFileName(key)) ? File.ReadAllText(GetFileName(key)) : string.Empty);

        public static bool DoesFileExist(string key) => _cache.ContainsKey(key) || File.Exists(GetFileName(key));

        public static void Set(string key, string value)
        {
            if (ReadFromFile(key) != value) File.WriteAllText(GetFileName(key), value);
            _cache[key] = value;
        }

        private static string GetSavedDirectoryPath(string fileKey)
        {
            var settingsPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{AppDataSettingsFolderName}\\{fileKey}{SaveFileExtension}";
            if (!File.Exists(settingsPath)) SaveToFile(settingsPath, GetPathFromUser(fileKey));
            return File.ReadAllText(settingsPath);
        }

        private static void SaveToFile(string path, string @string)
        {
            var directory = path.Substring(0, path.LastIndexOf("\\", StringComparison.InvariantCulture) + 1);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(path, @string);
        }

        private static string GetPathFromUser(string pathKey)
        {
            Console.WriteLine($"Enter the path for {pathKey}:");
            return Console.ReadLine();
        }

        private static string GetFileName(string key) => $"{SavePath}{key}{SaveFileExtension}";
    }
}