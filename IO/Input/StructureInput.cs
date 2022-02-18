using Structure.IO.Output;
using Structure.IO.Persistence;
using Structure.Structure;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Structure.IO.Input
{
    public class StructureInput : IProgramInput
    {
        protected ChainedInput InputSource { get; set; }

        public StructureInput(StructureIO io, INewsPrinter newsPrinter)
        {
            InitializeNewInputFromSavedSessions(io, newsPrinter);
            AddRecordingInputForEmptySaveSession();
        }

        protected void InitializeNewInputFromSavedSessions(StructureIO io, INewsPrinter newsPrinter)
        {
            InputSource = new ChainedInput();
            InputSource.AddAction(() => SetToLoadMode(io));
            var savedDataSessions = SavedSessionUtilities.LoadSavedDataSessions();
            var sessionsInputs = savedDataSessions.Select(x => new PredeterminedInput(x));
            sessionsInputs.All(x => InputSource.AddInput(x));
            InputSource.AddAction(() => SetToUserMode(io, newsPrinter));
        }

        private void AddRecordingInputForEmptySaveSession()
        {
            var nextDataSession = SavedSessionUtilities.LoadNextEmptyDataSession();
            var recordedUserInputSource = (IProgramInput)new RecordingInput(new ConsoleInput(), nextDataSession);
            InputSource.AddInput(recordedUserInputSource);
        }

        public bool IsKeyAvailable() => InputSource.IsKeyAvailable();

        public ProgramInputData ReadKey() => InputSource.ReadKey();

        protected static void SetToLoadMode(StructureIO io)
        {
            Contract.Requires(io != null);
            io.Run(() =>
            {
                io.Write("Loading...");
                io.ProgramOutput = new NoOpOutput();
                io.SkipUnescesscaryOperations = true;
            });
        }

        protected static void SetToUserMode(StructureIO io, INewsPrinter newsPrinter)
        {
            Contract.Requires(io != null);
            newsPrinter?.ClearNews();
            io.ProgramOutput = new ConsoleOutput();
            io.CurrentTime.SetToRealTime();
            io.Refresh();
            io.SkipUnescesscaryOperations = false;
        }

        public void RemoveLastReadKey() => InputSource.RemoveLastReadKey();
    }
}