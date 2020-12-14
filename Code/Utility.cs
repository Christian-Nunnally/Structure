using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Structure
{
    public static class Utility
    {
        private static IEnumerable<int> _cachedCodeLengths;
        private static int _cachedNumberOfFiles;

        public static int CodeLength => CodeLengths.Sum();

        public static IEnumerable<int> CodeLengths => _cachedCodeLengths is null
            ? (_cachedCodeLengths = Directory.GetFiles(Data.CodePath, "*.cs", new EnumerationOptions() { RecurseSubdirectories = true })
                .Select(f => File.ReadAllText(f)
                                .Where(x => !char.IsWhiteSpace(x))
                                .Count()))
            : _cachedCodeLengths;

        public static int NumberOfCodeFiles => _cachedNumberOfFiles == 0
            ? (_cachedNumberOfFiles = Directory.GetFiles(Data.CodePath, "*.cs", new EnumerationOptions() { RecurseSubdirectories = true }).Count())
            : _cachedNumberOfFiles;

        public static int XPForNextLevel => ExperienceForLevel(Data.Level + 1, 10, 75, 25);

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