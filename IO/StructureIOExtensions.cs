using System;

namespace Structur.IO
{
    public static class StructureIOExtensions
    {
        public static void ReadInteger(this StructureIO io, string prompt, Action<int> continuation)
        {
            void continueWhenInteger(string x)
            {
                if (int.TryParse(x, out var integer)) continuation(integer);
                else
                {
                    io.Write($"'{x}' is not a valid integer.");
                    io.Run(() => io.ReadInteger(prompt, continuation));
                }
            }
            io.Write(prompt);
            io.Read(continueWhenInteger, KeyGroups.NoKeys, new[] { ConsoleKey.Enter });
        }
    }
}
