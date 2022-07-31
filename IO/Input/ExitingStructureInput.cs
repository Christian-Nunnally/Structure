using Structur.IO.Persistence;
using Structur.Program;
using Structur.Program.Utilities;
using System;
using System.Linq;

namespace Structur.IO.Input
{
    public class ExitingStructureInput : IProgramInput
    {
        public ExitingStructureInput(StructureProgram program)
        {
            InputSource = new ChainedInput();
            var savedDataSessions = SavedSessionUtilities.LoadSavedDataSessions();
            var sessionsInputs = savedDataSessions.Select(x => new PredeterminedInput(x));
            sessionsInputs.All(x => InputSource.AddInput(x));
            InputSource.AddAction(() => program.Exit = true);
            InputSource.AddAction(() => throw new InvalidProgramException());
        }

        public ChainedInput InputSource { get; }

        public bool IsKeyAvailable() => InputSource.IsKeyAvailable();

        public ProgramInputData ReadKey() => InputSource.ReadKey();

        public void RemoveLastReadKey() { }
    }
}