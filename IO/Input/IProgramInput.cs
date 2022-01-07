using System;

namespace Structure.Code
{
    public interface IProgramInput
    {
        public ProgramInputData ReadKey();

        public ProgramInputData ReadKey(ConsoleKey[] allowedKeys);

        public bool IsKeyAvailable();
    }
}