using System;
using System.Threading;

namespace Structure.Code
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var level = Data.GetLevel();
            var xpForNextLevel = Utility.ExperienceForLevel(level + 1, 10, 150, 13);
            var codeLength = Utility.GetCodeLength();

            IncrementDebt(codeLength);

            while (true)
            {
                var points = Data.GetPoints();
                Console.WriteLine($"Progress: {points}/{xpForNextLevel}");
                Console.WriteLine("Type task and press enter when complete.");
                var task = Console.ReadLine();
                Console.Clear();
                Console.WriteLine(task);
                Console.ReadLine();
                Console.WriteLine("Point gained!");
                Thread.Sleep(500);
                points++;
                Data.SetPoints(points);
                Console.Clear();
                if (points >= xpForNextLevel)
                {
                    Console.WriteLine("You have enough points to level up, would you like to? (y/n)");
                    var result = Console.ReadLine();
                    if (result == "y")
                    {
                        points -= xpForNextLevel;
                        Data.SetPoints(points);
                        level++;
                        Data.SetLevel(level);
                        break;
                    }
                }
            }
        }

        private static void IncrementDebt(int codeLength)
        {
            Console.WriteLine("Added debt per day = " + codeLength);
            if (Data.GetLastWriteTime("debt").Date != DateTime.Today.Date)
            {
                Data.SetDebt(Data.GetDebt() + codeLength);
            }
        }
    }
}