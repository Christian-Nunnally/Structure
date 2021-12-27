using Structure.Code.ProgramInput;
using System;
using System.Linq;

namespace Structure.Code
{
    public class StructureInput : IProgramInput
    {
        private readonly ChainedInput _inputSource;

        public StructureInput()
        {
            _inputSource = new ChainedInput();
            _inputSource.AddAction(SetToLoadMode);
            var (savedDataSessions, nextDataSession) = SavedSessionUtilities.LoadSavedDataSessions();
            var sessionsInputs = savedDataSessions.Select(x => new PredeterminedInput(x));
            sessionsInputs.All(x => _inputSource.AddInput(x));
            var recordedUserInputSource = new RecordingInput(new ConsoleInput(), nextDataSession);
            _inputSource.AddAction(SetToUserMode);
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

        private void SetToLoadMode()
        {
            IO.SupressConsoleCalls = true;
        }

        private void SetToUserMode()
        {
            IO.SupressConsoleCalls = false;
            CurrentTime.SetToRealTime();
            IO.Refresh();
        }
    }
}