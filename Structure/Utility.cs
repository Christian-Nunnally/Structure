using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Structure
{
    public static class Utility
    {
        public static int ExperienceForLevel(int level, int minimum, int factor, double doublingRate)
        {
            double total = 0;
            for (int i = 1; i < level; i++)
            {
                total += Math.Floor(i + factor * Math.Pow(2, i / doublingRate));
            }
            return (int)Math.Max(minimum, Math.Floor(total / 4));
        }

        internal static object XPForNextLevel(StructureData currentData)
        {
            return ExperienceForLevel(currentData.Level +1, 10, 75, 25);
        }

        public static void All<T>(this IEnumerable<T> collection, Action<T> action)
        {
            Contract.Requires(collection != null);
            foreach (var item in collection) action(item);
        }
    }
}