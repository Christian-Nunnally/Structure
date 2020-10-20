using System;
using System.IO;
using System.Linq;

namespace Structure.Code
{
    public static class Utility
    {
        public static int GetCodeLength()
        {
            var allCSFiles = Directory.GetFiles(Data.CodeDirectory, "*.cs", new EnumerationOptions() { RecurseSubdirectories = true });
            return allCSFiles.Sum(f => File.ReadAllText(f).Length);
        }

        public static int ExperienceForLevel(int level, int minimum, int factor, double doublingRate)
        {
            double total = 0;
            for (int i = 1; i < level; i++)
            {
                total += Math.Floor(i + factor * Math.Pow(2, i / doublingRate));
            }
            return (int)Math.Max(minimum, Math.Floor(total / 4));
        }
    }
}