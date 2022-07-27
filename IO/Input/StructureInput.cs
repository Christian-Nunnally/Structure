using Structure.IO.Output;
using Structure.IO.Persistence;
using Structure.Structure.Utility;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Structure.IO.Input
{
    public class StructureInput : IProgramInput
    {
        private Stopwatch _stopwatch;

        protected ChainedInput InputSource { get; set; }

        public StructureInput(StructureIO io, IProgramInput sourceInput, INewsPrinter newsPrinter)
        {
            InitializeNewInputFromSavedSessions(io, newsPrinter);
            AddRecordingInputForEmptySaveSession(sourceInput);
        }

        protected void InitializeNewInputFromSavedSessions(StructureIO io, INewsPrinter newsPrinter)
        {
            InputSource = new ChainedInput();
            InputSource.AddAction(() => SetToLoadMode(io, newsPrinter));
            var sessionsInputs = CreateInputsFromSavedSessions();
            sessionsInputs.All(x => InputSource.AddInput(x));
            InputSource.AddAction(() => SetToUserMode(io, newsPrinter));
        }

        private static IEnumerable<PredeterminedInput> CreateInputsFromSavedSessions()
        {
            var savedDataSessions = SavedSessionUtilities.LoadSavedDataSessions();
            var sessionsInputs = savedDataSessions.Select(x => new PredeterminedInput(x));
            return sessionsInputs;
        }

        private void AddRecordingInputForEmptySaveSession(IProgramInput sourceInput)
        {
            var nextDataSession = SavedSessionUtilities.LoadNextEmptyDataSession();
            var recordedUserInputSource = (IProgramInput)new RecordingInput(sourceInput, nextDataSession);
            InputSource.AddInput(recordedUserInputSource);
        }

        public bool IsKeyAvailable() => InputSource.IsKeyAvailable();

        public ProgramInputData ReadKey() => InputSource.ReadKey();

        protected void SetToLoadMode(StructureIO io, INewsPrinter newsPrinter)
        {
            io.ClearBuffer();
            io.Write("Loading...");
            io.ClearStaleOutput();
            io.ProgramOutput = new NoOpOutput();
            io.SkipUnescesscaryOperations = true;
            newsPrinter.Disable();
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        protected void SetToUserMode(StructureIO io, INewsPrinter newsPrinter)
        {
            newsPrinter.Enable();
            io.ProgramOutput = new ConsoleOutput();
            io.CurrentTime.SetToRealTime();
            var buffer = io.CurrentBuffer.ToString();
            io.ClearBuffer();
            io.WriteNoLine(buffer);
            io.ClearStaleOutput();
            io.SkipUnescesscaryOperations = false;
            _stopwatch.Stop();
            newsPrinter.EnqueueNews($"Load took {_stopwatch.ElapsedMilliseconds}ms");
        }

        public void RemoveLastReadKey() => InputSource.RemoveLastReadKey();
    }
}