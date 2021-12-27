using System;
using System.Linq;

namespace Structure.Code
{
    public class ConsoleInput : IProgramInput
    {
        public bool IsKeyAvailable() => Console.KeyAvailable;

        public ProgramInputData ReadKey(ConsoleKeyInfo[] allowedKeys)
        {
            var key = ReadKey();
            while (!allowedKeys.Contains(key.GetKeyInfo())) key = ReadKey();
            return key;
        }

        public ProgramInputData ReadKey()
        {
            var key = Console.ReadKey(true);
            return new ProgramInputData(key, DateTime.Now);
        }
    }
}