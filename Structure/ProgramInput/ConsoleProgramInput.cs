using System;

namespace Structure.Code
{
    public class ConsoleProgramInput : IProgramInput
    {
        public bool IsKeyAvailable() => Console.KeyAvailable;

        public ConsoleKeyInfo ReadKey() => Console.ReadKey(true);
    }
}