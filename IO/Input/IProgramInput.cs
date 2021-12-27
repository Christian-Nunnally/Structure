using System;

namespace Structure.Code
{
    public interface IProgramInput
    {
        public ProgramInputData ReadKey();

        public ProgramInputData ReadKey(ConsoleKeyInfo[] allowedKeys);

        public bool IsKeyAvailable();
    }
}