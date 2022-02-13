using System;

namespace Structure.IO.Input
{
    public interface IProgramInput
    {
        public ProgramInputData ReadKey();

        public bool IsKeyAvailable();
    }
}