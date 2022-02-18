using Structure.IO.Output;
using Structure.IO.Persistence;
using Structure.Structure;
using System.Collections.Generic;
using System.Linq;

namespace Structure.IO.Input
{
    public class StructureInput : IProgramInput
    {
        private List<IBackgroundProcess> _savedBackgroundProcesses;

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

        private void AddRecordingInputForEmptySaveSession()
        {
            var nextDataSession = SavedSessionUtilities.LoadNextEmptyDataSession();
            var recordedUserInputSource = (IProgramInput)new RecordingInput(new ConsoleInput(), nextDataSession);
            InputSource.AddInput(recordedUserInputSource);
        }

        public bool IsKeyAvailable() => InputSource.IsKeyAvailable();

        public ProgramInputData ReadKey() => InputSource.ReadKey();

        protected void SetToLoadMode(StructureIO io)
        {
            io.Clear(true);
            io.Write("Loading...");
            io.ProgramOutput = new NoOpOutput();
            io.SkipUnescesscaryOperations = true;
            _savedBackgroundProcesses = io.BackgroundProcesses.ToList();
            _savedBackgroundProcesses.Clear();
        }

        protected void SetToUserMode(StructureIO io, INewsPrinter newsPrinter)
        {
            newsPrinter.Disable();
            io.ProgramOutput = new ConsoleOutput();
            io.CurrentTime.SetToRealTime();
            io.Refresh();
            io.SkipUnescesscaryOperations = false;
            io.BackgroundProcesses.AddRange(_savedBackgroundProcesses);
        }

        public void RemoveLastReadKey() => InputSource.RemoveLastReadKey();
    }
}