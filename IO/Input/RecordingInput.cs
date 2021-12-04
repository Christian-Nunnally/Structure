using System;

namespace Structure.Code
{
    public class RecordingInput : IProgramInput
    {
        private readonly IProgramInput _inputSource;
        private readonly PersistedList<ProgramInputData> _logDestiation;

        public RecordingInput(IProgramInput inputSource, PersistedList<ProgramInputData> logDestiation)
        {
            _inputSource = inputSource;
            _logDestiation = logDestiation;
        }

        public bool IsKeyAvailable() => _inputSource.IsKeyAvailable();

        public ConsoleKeyInfo ReadKey()
        {
            var key = _inputSource.ReadKey();
            var keyData = new ProgramInputData(key, DateTime.Now);
            _logDestiation.Add(keyData);
            return key;
        }
    }
}