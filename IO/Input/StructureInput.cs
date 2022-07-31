using Structur.IO.Output;
using Structur.IO.Persistence;
using Structur.Program.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Structur.IO.Input
{
    public class StructureInput : IProgramInput
    {
        private Stopwatch _stopwatch;
        private readonly IProgramOutput _outputToSwitchTo;

        protected ChainedInput InputSource { get; set; }

        public StructureInput(StructureIO io, IProgramInput sourceInput, IProgramOutput outputToSwitchTo, INewsPrinter newsPrinter)
        {
            InitializeNewInputFromSavedSessions(io, newsPrinter);
            AddRecordingInputForEmptySaveSession(sourceInput);
            _outputToSwitchTo = outputToSwitchTo;
        }

        protected void InitializeNewInputFromSavedSessions(StructureIO io, INewsPrinter newsPrinter)
        {
            InputSource = new ChainedInput();
            InputSource.AddAction(() => SetToLoadMode(io, newsPrinter));
            var sessionsInputs = CreateInputsFromSavedSessions();

            int iteration = 0;
            double percentFactor = 0.0;
            int count = Math.Max(1, sessionsInputs.Count());
            int skip = 20;
            foreach (var sessionInputs in sessionsInputs)
            {
                percentFactor += 100;
                var temp = percentFactor / count;
                if (iteration++ % skip == 0) InputSource.AddAction(() => UpdateLoadingPercent(io, temp));
                InputSource.AddInput(sessionInputs);
            }
            InputSource.AddAction(() => SetToUserMode(io, newsPrinter));
        }

        private void UpdateLoadingPercent(StructureIO io, double percent)
        {
            ClearAndForceWrite(io, $"Loading... {percent.ToString("0.00", new NumberFormatInfo())}% ");
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
            ClearAndForceWrite(io, "Loading... ");
            io.ProgramOutput = new NoOpOutput();
            io.SkipUnescesscaryOperations = true;
            newsPrinter.Disable();
            StartStopwatch();
        }

        protected void SetToUserMode(StructureIO io, INewsPrinter newsPrinter)
        {
            newsPrinter.Enable();
            io.ProgramOutput = _outputToSwitchTo;
            io.CurrentTime.SetToRealTime();
            var buffer = io.CurrentBuffer.ToString();
            ClearAndForceWrite(io, buffer);
            io.SkipUnescesscaryOperations = false;
            StopStopwatch(newsPrinter);
        }

        private void StartStopwatch()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        private void StopStopwatch(INewsPrinter newsPrinter)
        {
            _stopwatch.Stop();
            newsPrinter.EnqueueNews($"Load took {_stopwatch.ElapsedMilliseconds}ms");
        }

        private void ClearAndForceWrite(StructureIO io, string text)
        {
            var output = io.ProgramOutput;
            io.ProgramOutput = _outputToSwitchTo;
            io.ClearBuffer();
            io.WriteNoLine(text);
            io.ClearStaleOutput();
            io.ProcessAllBackgroundWork();
            io.ProgramOutput = output;
        }

        public void RemoveLastReadKey() => InputSource.RemoveLastReadKey();
    }
}