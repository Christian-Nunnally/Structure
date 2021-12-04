using Structure.Code.ProgramInput;
using System;
using System.Linq;

namespace Structure.Code
{
    public class StructureProgramInput : IProgramInput
    {
        private readonly ChainedInput _inputSource;

        public StructureProgramInput()
        {
            _inputSource = new ChainedInput();
            _inputSource.AddAction(SetToLoadMode);
            var (savedDataSessions, nextDataSession) = SavedSessionUtilities.LoadSavedDataSessions();
            var sessionsInputs = savedDataSessions.Select(x => new PredeterminedInput(x));
            sessionsInputs.All(x => _inputSource.AddInput(x));
            var recordedUserInputSource = new RecordingProgramInput(new ConsoleProgramInput(), nextDataSession);
            _inputSource.AddAction(SetToUserMode);
            _inputSource.AddInput(recordedUserInputSource);
        }

        public bool IsKeyAvailable() => _inputSource.IsKeyAvailable();

        public ConsoleKeyInfo ReadKey() => _inputSource.ReadKey();

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