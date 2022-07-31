using System;

namespace Structur.IO.Input
{
    public class ConsoleInput : IProgramInput
    {
        public bool IsKeyAvailable() => Console.KeyAvailable;

        public ProgramInputData ReadKey()
        {
            var key = Console.ReadKey(true);
            return new ProgramInputData(key, DateTime.Now);
        }

        public void RemoveLastReadKey()
        {
        }
    }
}