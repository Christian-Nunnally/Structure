using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        private static string savePath;
        private static string codePath;
        private static string solutionFilePath;

        public static int XP { get => Get(nameof(XP)); set => Set(nameof(XP), value); }
        public static int Points { get => Get(nameof(Points)); set => Set(nameof(Points), value); }
        public static int Toxins { get => Get(nameof(Toxins)); set => Set(nameof(Toxins), value); }
        public static int Prestiege { get => Get(nameof(Prestiege)); set => Set(nameof(Prestiege), value); }
        public static int Level { get => Get(nameof(Level)); set => Set(nameof(Level), value); }
        public static int Grass { get => Get(nameof(Grass)); set => Set(nameof(Grass), value); }
        public static int CharacterBonus { get => Get(nameof(CharacterBonus)); set => Set(nameof(CharacterBonus), value); }
        public static int LastCodeLength { get => Get(nameof(LastCodeLength)); set => Set(nameof(LastCodeLength), value); }
        public static int LifetimePrestiege { get => Get(nameof(LifetimePrestiege)); set => Set(nameof(LifetimePrestiege), value); }
        public static int CharacterBonusPerFile { get => Get(nameof(CharacterBonusPerFile)); set => Set(nameof(CharacterBonusPerFile), value); }
        public static IList<string> ActiveTaskList => GetCollection(nameof(ActiveTaskList));
        public static IList<string> CompletedTaskList => GetCollection(nameof(CompletedTaskList));
        public static IList<string> EnabledModules => GetCollection(nameof(EnabledModules));
        public static string CodePath => codePath ?? (codePath = GetSavedDirectoryPath(nameof(CodePath)));
        public static string SavePath => savePath ?? (savePath = GetSavedDirectoryPath(nameof(SavePath)));

        public static string SolutionFilePath => solutionFilePath ?? (solutionFilePath = GetSavedDirectoryPath(nameof(SolutionFilePath)));

        public static DateTime GetLastWriteTime(string key) => File.GetLastWriteTime(GetFileName(key));

        public static int Get(string key)
        {
            var fileName = GetFileName(key);
            return _intCache.TryGetValue(key, out var value)
                ? value
                : (_intCache[key] = File.Exists(fileName) && int.TryParse(File.ReadAllText(fileName), out var x) ? x : 0);
        }

        public static void Set(string key, int value)
        {
            var oldValue = Get(key);
            if (oldValue != value)
            {
                var difference = value - oldValue;
                var prefix = difference > 0 ? "+" : "";
                IO.Write($"{prefix}{difference} {key}");
                _intCache[key] = value;
                File.WriteAllText(GetFileName(key), value.ToString());
            }
        }

        private static ObservableCollection<string> GetCollection(string key)
        {
            if (!_collectionCache.TryGetValue(key, out var collection))
            {
                var fileName = GetFileName(key);
                collection = File.Exists(fileName)
                   ? new ObservableCollection<string>(File.ReadAllText(fileName).Split(',').Where(x => !string.IsNullOrWhiteSpace(x)))
                   : new ObservableCollection<string>();
                collection.CollectionChanged += (s, e) => CollectionChanged(key, e);
                _collectionCache[key] = collection;
            }
            return collection;
        }

        private static void CollectionChanged(string key, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems == null && (e.NewItems?.Count ?? 0) > 0)
            {
                var newItems = new string[e.NewItems.Count];
                e.NewItems.CopyTo(newItems, 0);
                File.AppendAllText(GetFileName(key), "," + string.Join(",", newItems));
            }
            else
            {
                File.WriteAllText(GetFileName(key), string.Join(",", _collectionCache[key]));
            }
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