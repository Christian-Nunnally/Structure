﻿using Structure.Code.ProgramInput;
using Structure.IO;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Structure.Code
{
    public class StructureInput : IProgramInput
    {
        protected ChainedInput InputSource { get; set; }

        public StructureInput(StructureIO io, NewsPrinter newsPrinter)
        {
            InitializeNewInputFromSavedSessions(io, newsPrinter);
            AddRecordingInputForEmptySaveSession();
        }

        protected void InitializeNewInputFromSavedSessions(StructureIO io, NewsPrinter newsPrinter)
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

        public ProgramInputData ReadKey()
        {
            return InputSource.ReadKey();
        }

        protected static void SetToLoadMode(StructureIO io)
        {
            Contract.Requires(io != null);
            io.ProgramOutput = new NoOpOutput();
        }

        protected static void SetToUserMode(StructureIO io, NewsPrinter newsPrinter)
        {
            Contract.Requires(io != null);
            newsPrinter?.ClearNews();
            io.ProgramOutput = new ConsoleOutput();
            io.CurrentTime.SetToRealTime();
            io.Refresh();
        }
    }
}