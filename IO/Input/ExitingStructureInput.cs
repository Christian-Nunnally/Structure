using Structure.IO.Persistence;
using Structure.Structure;
using System;
using System.Linq;
using System.Threading;

namespace Structure.IO.Input
{
    public class ExitingStructureInput : IProgramInput
    {
        public ExitingStructureInput(StructureProgram program)
        {
            InputSource = new ChainedInput();
            var savedDataSessions = SavedSessionUtilities.LoadSavedDataSessions();
            var sessionsInputs = savedDataSessions.Select(x => new PredeterminedInput(x));
            sessionsInputs.All(x => InputSource.AddInput(x));
            InputSource.AddAction(() => throw new InvalidProgramException());
            //InputSource.AddInput(new TestingProgramInput(program));
        }

        public ChainedInput InputSource { get; }

        public bool IsKeyAvailable() => InputSource.IsKeyAvailable();

        public ProgramInputData ReadKey() => InputSource.ReadKey();

        public void RemoveLastReadKey() { }

        private class TestingProgramInput : IProgramInput
        {
            private PredeterminedInput _currentInput;
            private readonly StructureProgram _program;

            public TestingProgramInput(StructureProgram program)
            {
                _program = program;
            }

            public bool IsKeyAvailable()
            {
                ResetInput();
                return true;
            }

            public ProgramInputData ReadKey(ConsoleKey[] allowedKeys)
            {
                return ReadKey();
            }

            public ProgramInputData ReadKey()
            {
                ResetInput();
                return _currentInput.ReadKey();
            }

            private void ResetInput()
            {
                _program.Exit = true;
                if (_currentInput == null || !_currentInput.IsKeyAvailable())
                {
                    _currentInput = new PredeterminedInput(new[] { new ProgramInputData(new ConsoleKeyInfo('\u2386', ConsoleKey.Enter, false, false, false), DateTime.Now) });
                }
            }

            public void RemoveLastReadKey()
            {
            }
        }
    }
}