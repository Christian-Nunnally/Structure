using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Structure
{
    public static class Utility
    {
        private static IEnumerable<(string FileName, int Length)> _codeLengths;

        public static int CodeLength => CodeLengths.Select(x => x.Length).Sum();

        public static IEnumerable<(string FileName, int Length)> CodeLengths => _codeLengths is null
            ? (_codeLengths = Directory.GetFiles(FileIO.CodePath, "*.cs", new EnumerationOptions() { RecurseSubdirectories = true })
                .Select(f => (Path.GetFileName(f).Split('.').First(), File.ReadAllText(f)
                                .Where(x => !char.IsWhiteSpace(x))
                                .Count())))
            : _codeLengths;

        public static int XPForNextLevel => ExperienceForLevel(CommonData.Level + 1, 10, 75, 25);

        public static int ExperienceForLevel(int level, int minimum, int factor, double doublingRate)
        {
            double total = 0;
            for (int i = 1; i < level; i++)
            {
                total += Math.Floor(i + factor * Math.Pow(2, i / doublingRate));
            }
            return (int)Math.Max(minimum, Math.Floor(total / 4));
        }

        public static void All<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection) action(item);
        }
    }
}