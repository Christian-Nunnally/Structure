using System;
using System.IO;
using System.Linq;

namespace Structure
{
    public static class Utility
    {
        private static int? _cachedCodeLength;

        public static int GetCodeLength()
        {
            if (_cachedCodeLength == null)
            {
                var allCSFiles = Directory.GetFiles(Data.CodeDirectory, "*.cs", new EnumerationOptions() { RecurseSubdirectories = true });
                _cachedCodeLength = allCSFiles.Sum(f => File.ReadAllText(f).Where(x => !char.IsWhiteSpace(x)).Count());
            }
            return _cachedCodeLength ?? 0;
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