using System;

namespace Structure.Code
{
    public interface IProgramInput
    {
        public ConsoleKeyInfo ReadKey();

        public bool IsKeyAvailable();
    }
}