using Structure.Code.ProgramInput;
using System;
using System.Linq;

namespace Structure.Code
{
    public class StructureInput : IProgramInput
    {
        private const bool DEVELOPMENT_MODE = false;

        private ChainedInput _inputSource;

        public StructureInput()
        {
        }

        public void InitializeStructureInput(StructureIO io)
        {
            _inputSource = new ChainedInput();
            _inputSource.AddAction(() => SetToLoadMode(io));
            var (savedDataSessions, nextDataSession) = SavedSessionUtilities.LoadSavedDataSessions();
            var sessionsInputs = savedDataSessions.Select(x => new PredeterminedInput(x));
            sessionsInputs.All(x => _inputSource.AddInput(x));
            var recordedUserInputSource = (IProgramInput)new RecordingInput(new ConsoleInput(), nextDataSession);
            if (DEVELOPMENT_MODE) recordedUserInputSource = new ConsoleInput();
            _inputSource.AddAction(() => SetToUserMode(io));
            _inputSource.AddInput(recordedUserInputSource);
        }

        public bool IsKeyAvailable() => _inputSource.IsKeyAvailable();

        public ProgramInputData ReadKey(ConsoleKeyInfo[] allowedKeys)
        {
            return _inputSource.ReadKey(allowedKeys);
        }

        public ProgramInputData ReadKey()
        {
            return _inputSource.ReadKey();
        }

        private static void SetToLoadMode(StructureIO io)
        {
            io.SetOutput(new NoOpOutput());
        }

        private static void SetToUserMode(StructureIO io)
        {
            io.SetOutput(new ConsoleOutput());
            io.CurrentTime.SetToRealTime();
            io.Refresh();
        }
    }
}