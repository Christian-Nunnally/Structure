using Structur.Program;
using Structur.Program.Utilities;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Structur.IO.Persistence
{
    /// <summary>
    /// Treats the file system as a dictionary of key/value pairs in order to add a layer of caching.
    /// </summary>
    public static class StructureFileIO
    {
        public const string SaveFileExtension = ".structure";
        public const string AppDataSettingsFolderName = "Structure";
        private static readonly ConcurrentDictionary<string, string> _cache = new();
        private static string savePath;

        public static string SavePath => savePath ??= GetSavedPath();

        public static string ReadFromFile(string key) => _cache.TryGetValue(key, out var value)
            ? value : (_cache[key] = GetFileName(key).SafelyReadAllTextFromFile());

        public static bool DoesFileExist(string key) => _cache.ContainsKey(key) || File.Exists(GetFileName(key));

        public static void Set(string key, string value)
        {
            if (ReadFromFile(key) != value) File.WriteAllText(GetFileName(key), value);
            _cache[key] = value;
        }

        private static string GetSavedPath()
        {
            var settings = Settings.ReadSettings();
            if (settings.SavePath == null) throw new InvalidOperationException($"Supply a save path in the {Settings.DefaultSettingsFile} file.");
            return settings.SavePath;
        }

        private static string GetFileName(string key) => $"{SavePath}{key}{SaveFileExtension}";
    }
}