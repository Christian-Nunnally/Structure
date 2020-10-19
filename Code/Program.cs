using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace Structure
{
    internal class Program
    {
        public const string SaveFileExtension = ".structure";
        public static string SaveDirectory = null;

        public static int GetValue(string key)
        {
            var fileName = GetSaveFileName(key);
            return File.Exists(fileName) && int.TryParse(File.ReadAllText(fileName), out var points) ? points : 0;
        }

        public static void SetValue(string key, int value)
        {
            string fileName = GetSaveFileName(key);
            File.WriteAllText(fileName, value.ToString());
        }

        public static int GetDebt() => GetValue("debt");

        public static int GetPoints() => GetValue("points");

        public static void SetPoints(int points) => SetValue("points", points);

        public static void SetDebt(int debt) => SetValue("debt", debt);

        public static void SetLevel(int level) => SetValue("level", level);

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

        private static void Main(string[] args)
        {
            var level = GetValue("level");
            var xpForNextLevel = ExperienceForLevel(level + 1);
            string debtFileName = GetSaveFileName("debt");
            if (File.GetLastWriteTime(debtFileName).Date != DateTime.Today.Date)
            {
                SetDebt(GetDebt() + GetCodeLength());
            }

            var codeLength = GetCodeLength();
            Console.WriteLine("Code length = " + codeLength);
            while (true)
            {
                var points = GetPoints();
                Console.WriteLine($"Progress: {points}/{xpForNextLevel}");
                Console.WriteLine("Type task and press enter when complete.");
                var task = Console.ReadLine();
                Console.Clear();
                Console.WriteLine(task);
                Console.ReadLine();
                Console.WriteLine("Point gained!");
                Thread.Sleep(500);
                points++;
                SetPoints(points);
                Console.Clear();
                if (points >= xpForNextLevel)
                {
                    Console.WriteLine("You have enough points to level up, would you like to? (y/n)");
                    var result = Console.ReadLine();
                    if (result == "y")
                    {
                        points -= xpForNextLevel;
                        SetPoints(points);
                        level++;
                        SetLevel(level);
                        break;
                    }
                }
            }
        }

        private static int GetCodeLength()
        {
            var temp = Directory.GetFiles(".");
            var allCSFiles = Directory.GetFiles(@"C:\Users\chris\source\repos\Structure\Structure\Code", "*.cs", new EnumerationOptions() { RecurseSubdirectories = true });
            return allCSFiles.Sum(f => File.ReadAllText(f).Length);
        }

        private static int ExperienceForLevel(int level)
        {
            double total = 0;
            for (int i = 1; i < level; i++)
            {
                total += Math.Floor(i + 150 * Math.Pow(2, i / 13.0));
            }

            return (int)Math.Floor(total / 4);
        }
    }
}