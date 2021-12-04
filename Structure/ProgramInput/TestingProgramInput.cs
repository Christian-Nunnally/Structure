using System;

namespace Structure.Code.ProgramInput
{
    public class TestingProgramInput : IProgramInput
    {
        PredeterminedProgramInput _currentInput;

        public bool IsKeyAvailable()
        {
            ResetInput();
            return true;
        }

        public ConsoleKeyInfo ReadKey()
        {
            ResetInput();
            return _currentInput.ReadKey();
        }

        private void ResetInput()
        {
            Program.Exit = true;
            if (_currentInput == null || !_currentInput.IsKeyAvailable())
            {
                _currentInput = new PredeterminedProgramInput(new[] { new ProgramInputData(new ConsoleKeyInfo('\u2386', ConsoleKey.Enter, false, false, false), DateTime.Now) });
            }
        }
    }
}
