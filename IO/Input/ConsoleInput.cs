using System;

namespace Structur.IO.Input
{
    public class ConsoleInput : IProgramInput
    {
        public bool IsInputAvailable() => Console.KeyAvailable;

        public ProgramInputData ReadInput()
        {
            var key = Console.ReadKey(true);
            return new ProgramInputData(key, DateTime.Now);
        }

        public void RemoveLastInput()
        {
        }
    }
}