using System;

namespace Structure.Code
{
    public interface IProgramInput
    {
        public ProgramInputData ReadKey();

        public bool IsKeyAvailable();
    }
}