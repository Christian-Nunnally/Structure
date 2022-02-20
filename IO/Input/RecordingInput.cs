using Structure.IO.Persistence;

namespace Structure.IO.Input
{
    public class RecordingInput : IProgramInput
    {
        private readonly IProgramInput _inputSource;
        private readonly PersistedList<ProgramInputData> _recordedInputs;
        private ProgramInputData _lastInput;

        public RecordingInput(IProgramInput inputSource, PersistedList<ProgramInputData> logDestiation)
        {
            _inputSource = inputSource;
            _recordedInputs = logDestiation;
        }

        public bool IsKeyAvailable() => _inputSource.IsKeyAvailable();

        public ProgramInputData ReadKey()
        {
            _lastInput = _inputSource.ReadKey();
            _recordedInputs.Add(_lastInput);
            return _lastInput;
        }

        public void RemoveLastReadKey()
        {
            _recordedInputs.Remove(_lastInput);
        }
    }
}