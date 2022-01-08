using Structure.Code.ProgramInput;
using Structure.IO;
using System;
using System.Linq;

namespace Structure.Code
{
    public class StructureInput : IProgramInput
    {
        private const bool DEVELOPMENT_MODE = true;
        private const bool NON_SAVE_MODE = false;

        private readonly ChainedInput _inputSource;

        public StructureInput(StructureIO io, NewsPrinter newsPrinter)
        {
            _inputSource = new ChainedInput();
            if (!NON_SAVE_MODE)
            {
                _inputSource.AddAction(() => SetToLoadMode(io));
                var (savedDataSessions, nextDataSession) = SavedSessionUtilities.LoadSavedDataSessions();
                var sessionsInputs = savedDataSessions.Select(x => new PredeterminedInput(x));
                sessionsInputs.All(x => _inputSource.AddInput(x));
                var recordedUserInputSource = (IProgramInput)new RecordingInput(new ConsoleInput(), nextDataSession);
                if (DEVELOPMENT_MODE) recordedUserInputSource = new ConsoleInput();
                _inputSource.AddAction(() => SetToUserMode(io, newsPrinter));
                _inputSource.AddInput(recordedUserInputSource);
            }
            else
            {
                _inputSource.AddInput(new ConsoleInput());
            }
        }

        public bool IsKeyAvailable() => _inputSource.IsKeyAvailable();

        public ProgramInputData ReadKey()
        {
            return _inputSource.ReadKey();
        }

        private static void SetToLoadMode(StructureIO io)
        {
            io.ProgramOutput = new NoOpOutput();
        }

        private static void SetToUserMode(StructureIO io, NewsPrinter newsPrinter)
        {
            newsPrinter.ClearNews();
            io.ProgramOutput = new ConsoleOutput();
            io.CurrentTime.SetToRealTime();
            io.Refresh();
        }
    }
}