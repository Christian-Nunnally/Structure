using System;
using System.IO;

namespace Structure.Code
{
    public static class Data
    {
        public const string SaveFileExtension = ".structure";

        public const string AppDataSettingsFolderName = "Structure";

        private static string saveDirectory;

        private static string codeDirectory;

        private static string repoDirectory;

        public enum Key
        {
            Points,
            Toxins,
            Prestiege,
            Level,
            Grass,
            CharacterBonus,
        }

        public static string SaveDirectory
        {
            get => string.IsNullOrEmpty(saveDirectory) ? (saveDirectory = GetOrCreatePersistantDirectoryPath(nameof(SaveDirectory))) : saveDirectory;
            set => saveDirectory = value;
        }

        public static string CodeDirectory
        {
            get => string.IsNullOrEmpty(codeDirectory) ? (codeDirectory = GetOrCreatePersistantDirectoryPath(nameof(CodeDirectory))) : codeDirectory;
            set => codeDirectory = value;
        }

        public static string RepoDirectory
        {
            get => string.IsNullOrEmpty(repoDirectory) ? (repoDirectory = GetOrCreatePersistantDirectoryPath(nameof(RepoDirectory))) : repoDirectory;
            set => repoDirectory = value;
        }

        public static DateTime GetLastWriteTime(Key key) => File.GetLastWriteTime(GetSaveFileName(key));

        public static int Get(Key key)
        {
            var fileName = GetSaveFileName(key);
            return File.Exists(fileName) && int.TryParse(File.ReadAllText(fileName), out var points) ? points : 0;
        }

        public static void Set(Key key, int value) => File.WriteAllText(GetSaveFileName(key), value.ToString());

        private static string GetOrCreatePersistantDirectoryPath(string fileKey)
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var persistantDirectorySaveFilePath = $"{appDataFolder}\\{AppDataSettingsFolderName}\\{fileKey}{SaveFileExtension}";
            if (!File.Exists(persistantDirectorySaveFilePath))
            {
                var userResult = GetPathFromUser(fileKey);
                SaveStringToFile(persistantDirectorySaveFilePath, userResult);
                return userResult;
            }
            return File.ReadAllText(persistantDirectorySaveFilePath);
        }

        private static void SaveStringToFile(string saveFilePath, string savedString)
        {
            var directory = saveFilePath.Substring(0, saveFilePath.LastIndexOf("\\") + 1);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(saveFilePath, savedString);
        }

        private static string GetPathFromUser(string fileKey)
        {
            Console.WriteLine($"Enter the path for {fileKey}:");
            var userResult = Console.ReadLine();
            return userResult.EndsWith("\\") ? userResult : $"{userResult}\\";
        }

        private static string GetSaveFileName(Key key) => $"{SaveDirectory}{key}{SaveFileExtension}";
    }
}