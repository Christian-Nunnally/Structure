using System;

namespace Structure.Code
{
    public class ConsoleInput : IProgramInput
    {
        public bool IsKeyAvailable() => Console.KeyAvailable;

        public ConsoleKeyInfo ReadKey() => Console.ReadKey(true);
    }
}