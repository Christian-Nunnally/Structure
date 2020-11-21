using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Structure
{
    public static class Data
    {
        public const string SaveFileExtension = ".structure";
        public const string AppDataSettingsFolderName = "Structure";

        private static readonly Dictionary<string, int> _intCache = new Dictionary<string, int>();
        private static readonly Dictionary<string, ObservableCollection<string>> _collectionCache = new Dictionary<string, ObservableCollection<string>>();
        private static string saveDirectory;
        private static string codeDirectory;

        public static string SaveDirectory => saveDirectory ?? (saveDirectory = GetOrCreateSavedDirectoryPath(nameof(SaveDirectory)));

        public static string CodeDirectory => codeDirectory ?? (codeDirectory = GetOrCreateSavedDirectoryPath(nameof(CodeDirectory)));

        public static int Points { get => Get(nameof(Points)); set => Set(nameof(Points), value); }

        public static int Toxins { get => Get(nameof(Toxins)); set => Set(nameof(Toxins), value); }

        public static int Prestiege { get => Get(nameof(Prestiege)); set => Set(nameof(Prestiege), value); }

        public static int Level { get => Get(nameof(Level)); set => Set(nameof(Level), value); }

        public static int Grass { get => Get(nameof(Grass)); set => Set(nameof(Grass), value); }

        public static int CharacterBonus { get => Get(nameof(CharacterBonus)); set => Set(nameof(CharacterBonus), value); }

        public static int LastCodeLength { get => Get(nameof(LastCodeLength)); set => Set(nameof(LastCodeLength), value); }

        public static int LifetimePrestiege { get => Get(nameof(LifetimePrestiege)); set => Set(nameof(LifetimePrestiege), value); }

        public static IList<string> TaskList => GetCollectionFromFileOrCache(nameof(TaskList));

        public static DateTime GetLastWriteTime(string key) => File.GetLastWriteTime(GetSaveFileName(key));

        public static int Get(string key)
        {
            var fileName = GetSaveFileName(key);
            if (_intCache.TryGetValue(key, out var value)) return value;
            value = File.Exists(fileName) && int.TryParse(File.ReadAllText(fileName), out var points) ? points : 0;
            _intCache[key] = value;
            return value;
        }

        public static void Set(string key, int value)
        {
            _intCache[key] = value;
            File.WriteAllText(GetSaveFileName(key), value.ToString());
        }

        private static ObservableCollection<string> GetCollectionFromFileOrCache(string key)
        {
            if (!_collectionCache.TryGetValue(key, out var collection))
            {
                var fileName = GetSaveFileName(key);
                collection = File.Exists(fileName)
                   ? new ObservableCollection<string>(File.ReadAllText(fileName).Split(',').Where(x => !string.IsNullOrWhiteSpace(x)))
                   : new ObservableCollection<string>();
                collection.CollectionChanged += (s, e) => CollectionChanged(key);
                _collectionCache[key] = collection;
            }
            return collection;
        }

        private static void CollectionChanged(string key) => File.WriteAllText(GetSaveFileName(key), string.Join(",", _collectionCache[key]));

        private static string GetOrCreateSavedDirectoryPath(string fileKey)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var pathToSavePath = $"{appData}\\{AppDataSettingsFolderName}\\{fileKey}{SaveFileExtension}";
            if (!File.Exists(pathToSavePath))
            {
                var userResult = GetPathFromUser(fileKey);
                SaveToFile(pathToSavePath, userResult);
            }
            return File.ReadAllText(pathToSavePath);
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
            var userResult = Console.ReadLine();
            return userResult.EndsWith("\\") ? userResult : $"{userResult}\\";
        }

        private static string GetSaveFileName(string key) => $"{SaveDirectory}{key}{SaveFileExtension}";
    }
}