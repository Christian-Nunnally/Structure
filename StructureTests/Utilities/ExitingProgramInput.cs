using Structur.IO.Input;
using Structur.IO.Output;
using Structur.Program;
using System;

namespace StructureTests.Utilities
{
    public class ExitingProgramInput : IProgramInput
    {
        private PredeterminedInput _currentInput;
        private readonly ExitToken _programExitToken;
        private readonly ExitToken _readyExitToken;
        private readonly TextOutput _testOutput;

        public ExitingProgramInput(ExitToken programExitToken, ExitToken readyExitToken, TextOutput testOutput)
        {
            _programExitToken = programExitToken;
            _readyExitToken = readyExitToken;
            _testOutput = testOutput;
        }

        public bool IsInputAvailable()
        {
            if (!_readyExitToken.Exit) return false;
            ResetInput();
            return true;
        }

        public ProgramInputData ReadInput()
        {
            if (!_readyExitToken.Exit) throw new InvalidCastException(":(");
            ResetInput();
            _testOutput.Disable();
            return _currentInput.ReadInput();
        }

        private void ResetInput()
        {
            _programExitToken.Exit = true;
            if (_currentInput == null || !_currentInput.IsInputAvailable())
            {
                _currentInput = new PredeterminedInput(new[] { new ProgramInputData(new ConsoleKeyInfo('\u2386', ConsoleKey.Enter, false, false, false), DateTime.Now) });
            }
        }

        public void RemoveLastInput()
        {
        }
    }
}
