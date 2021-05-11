using System;
using System.Linq;

namespace Structure.Code
{
    public class StructureProgramInput : IProgramInput
    {
        private readonly ChainedProgramInput _inputSource;
        private int _loadedSession;

        public StructureProgramInput()
        {
            _inputSource = new ChainedProgramInput();
            _inputSource.AddAction(SetToLoadMode);
            var consoleKeyInfos = LoadNextInputDataSet();
            while (consoleKeyInfos.Any())
            {
                _inputSource.AddInput(new PredeterminedProgramInput(consoleKeyInfos));
                consoleKeyInfos = LoadNextInputDataSet();
            }
            var recordedUserInputSource = new RecordingProgramInput(new ConsoleProgramInput(), consoleKeyInfos);
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

        private PersistedList<ProgramInputData> LoadNextInputDataSet()
        {
            return new PersistedList<ProgramInputData>($"session-{_loadedSession++}", true);
        }
    }
}