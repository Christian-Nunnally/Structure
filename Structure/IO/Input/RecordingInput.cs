using Structur.IO.Persistence;

namespace Structur.IO.Input
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

        public bool IsInputAvailable() => _inputSource.IsInputAvailable();

        public ProgramInputData ReadInput()
        {
            _lastInput = _inputSource.ReadInput();
            _recordedInputs.Add(_lastInput);
            return _lastInput;
        }

        public void RemoveLastInput()
        {
            _recordedInputs.Remove(_lastInput);
        }
    }
}