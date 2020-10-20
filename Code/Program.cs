using System;
using System.Threading;

namespace Structure.Code
{
    internal class Program
    {
        public static bool _exiting;
        public static int _maxLevel;

        private static void Main(string[] args)
        {
            _maxLevel = int.Parse(args[0]);
            var level = Data.Get(Data.Key.Level);
            var xpForNextLevel = Utility.ExperienceForLevel(level + 1, 10, 150, 13);
            var codeLength = Utility.GetCodeLength();

            IncrementToxins(codeLength);
            PromptPrestiegeOptions();

            while (!_exiting)
            {
                var points = Data.Get(Data.Key.Points);
                Console.WriteLine($"Progress: {points}/{xpForNextLevel}");
                Console.WriteLine("Type task and press enter when complete.");
                var task = Console.ReadLine();
                Console.Clear();
                Console.WriteLine(task);
                Console.ReadLine();
                Data.Set(Data.Key.Points, points + 1);
                Console.WriteLine("Point gained!");
                Thread.Sleep(500);
                Console.Clear();
                TryToLevelUp();
            }
        }

        private static void PromptPrestiegeOptions()
        {
            var prestiegePoints = Data.Get(Data.Key.Prestiege);
            if (prestiegePoints > 0)
            {
                PromptToSpendPrestiegeOnGrass(prestiegePoints);
            }
        }

        private static void PromptToSpendPrestiegeOnGrass(int prestiegePoints)
        {
            Console.WriteLine($"You have {prestiegePoints} prestiege points.");
            Console.WriteLine("would you like to spend one on 10 blades of grass? (y/n)");
            while (Console.ReadLine() == "y" && prestiegePoints > 0)
            {
                Data.Set(Data.Key.Grass, Data.Get(Data.Key.Grass) + 10);
                prestiegePoints--;
                if (prestiegePoints == 0) break;
                Console.WriteLine($"You now have {prestiegePoints} prestiege points.");
                Console.WriteLine("would you like to spend one on 10 blades of grass? (y/n)");
            }
            Console.WriteLine($"You now have {Data.Get(Data.Key.Grass)} blades of grass.");
            Console.WriteLine($"Each blade of grass will reduce added toxins by 1 per day.");
            Data.Set(Data.Key.Prestiege, prestiegePoints);
        }

        private static void Prestiege()
        {
            Data.Set(Data.Key.Level, 0);
            Data.Set(Data.Key.Points, 0);
            Data.Set(Data.Key.Prestiege, Data.Get(Data.Key.Prestiege) + 1);
            _exiting = true;
        }

        private static void TryToLevelUp()
        {
            var points = Data.Get(Data.Key.Points);
            var level = Data.Get(Data.Key.Level);
            var xpForNextLevel = Utility.ExperienceForLevel(level + 1, 10, 150, 13);
            if (points >= xpForNextLevel)
            {
                if (level >= _maxLevel)
                {
                    PromptToPrestiege();
                }
                else
                {
                    PromptToLevelUp(points, level, xpForNextLevel);
                }
            }
        }

        private static void PromptToLevelUp(int points, int level, int xpForNextLevel)
        {
            Console.WriteLine("You have enough points to level up, would you like to? (y/n)");
            if (Console.ReadLine() == "y")
            {
                LevelUp(points, level, xpForNextLevel);
            }
        }

        private static void PromptToPrestiege()
        {
            Console.WriteLine("You have reached the max level. Would you like to restart and gain 1 prestiege point? (y/n)");
            if (Console.ReadLine() == "y")
            {
                Prestiege();
            }
        }

        private static void LevelUp(int points, int level, int xpForNextLevel)
        {
            points -= xpForNextLevel;
            Data.Set(Data.Key.Points, points);
            Data.Set(Data.Key.Level, level + 1);
            _exiting = true;
        }

        private static void IncrementToxins(int codeLength)
        {
            var grass = Data.Get(Data.Key.Grass);
            var addedToxins = Math.Max(0, codeLength - grass);
            Console.Write($"Added toxins per day = {addedToxins}{(grass > 0 ? $" ({grass} toxins being absorbed by grass blades)" : "")}");
            if (Data.GetLastWriteTime(Data.Key.Toxins).Date != DateTime.Today.Date)
            {
                Data.Set(Data.Key.Toxins, Data.Get(Data.Key.Toxins) + addedToxins);
            }
        }
    }
}