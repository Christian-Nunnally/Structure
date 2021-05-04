using System;
using System.Collections.Generic;
using System.IO;

namespace Structure
{
    public static class FileIO
    {
        public const string SaveFileExtension = ".structure";
        public const string AppDataSettingsFolderName = "Structure";
        private static Dictionary<string, string> _cache = new Dictionary<string, string>();
        private static string savePath;
        private static string codePath;
        private static string solutionFilePath;

        public static string CodePath => codePath ?? (codePath = GetSavedDirectoryPath(nameof(CodePath)));
        public static string SavePath => savePath ?? (savePath = GetSavedDirectoryPath(nameof(SavePath)));
        public static string SolutionFilePath => solutionFilePath ?? (solutionFilePath = GetSavedDirectoryPath(nameof(SolutionFilePath)));

        public static DateTime GetLastWriteTime(string key) => File.GetLastWriteTime(GetFileName(key));

        public static string ReadFromFile(string key) => _cache.TryGetValue(key, out var value) ? value
            : (_cache[key] = File.Exists(GetFileName(key)) ? File.ReadAllText(GetFileName(key)) : string.Empty);

        public static void Set(string key, string value)
        {
            if (ReadFromFile(key) != value) File.WriteAllText(GetFileName(key), value);
            _cache[key] = value;
        }

        public static void Append(string key, string value)
        {
            _cache[key] = ReadFromFile(key) + value;
            File.AppendAllText(GetFileName(key), _cache[key]);
        }

        private static string GetSavedDirectoryPath(string fileKey)
        {
            var settingsPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\{AppDataSettingsFolderName}\\{fileKey}{SaveFileExtension}";
            if (!File.Exists(settingsPath)) SaveToFile(settingsPath, GetPathFromUser(fileKey));
            return File.ReadAllText(settingsPath);
        }

        private static void SaveToFile(string path, string @string)
        {
            var directory = path.Substring(0, path.LastIndexOf("\\") + 1);
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