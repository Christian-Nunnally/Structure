using System;
using System.IO;

namespace Structure.Code
{
    public static class Data
    {
        public const string SaveFileExtension = ".structure";
        public static string SaveDirectory = null;

        public static int GetDebt() => GetValue("debt");

        public static int GetPoints() => GetValue("points");

        public static int GetLevel() => GetValue("level");

        public static void SetPoints(int points) => SetValue("points", points);

        public static void SetDebt(int debt) => SetValue("debt", debt);

        public static void SetLevel(int level) => SetValue("level", level);

        public static DateTime GetLastWriteTime(string key) => File.GetLastWriteTime(GetSaveFileName(key));

        private static int GetValue(string key)
        {
            var fileName = GetSaveFileName(key);
            return File.Exists(fileName) && int.TryParse(File.ReadAllText(fileName), out var points) ? points : 0;
        }

        private static void SetValue(string key, int value)
        {
            string fileName = GetSaveFileName(key);
            File.WriteAllText(fileName, value.ToString());
        }

        private static string GetSaveFileName(string key)
        {
            if (string.IsNullOrEmpty(SaveDirectory))
            {
                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var structureSettingsDirectory = $"{appDataFolder}\\Structure\\";
                var structureSettingsPath = $"{structureSettingsDirectory}\\settings.structure";
                if (!File.Exists(structureSettingsPath))
                {
                    Console.WriteLine("Enter a path for your structure save files to live:");
                    var path = Console.ReadLine();
                    path = path.EndsWith("//") ? path : $"{path}\\";
                    Directory.CreateDirectory(structureSettingsDirectory);
                    File.WriteAllText(structureSettingsPath, path);
                }
                SaveDirectory = File.ReadAllText(structureSettingsPath);
            }
            return $"{SaveDirectory}{key}{SaveFileExtension}";
        }
    }
}