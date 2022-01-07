﻿using System;
using System.Linq;

namespace Structure.Code
{
    public class RecordingInput : IProgramInput
    {
        private readonly IProgramInput _inputSource;
        private readonly PersistedListCollection<ProgramInputData> _recordedInputs;

        public RecordingInput(IProgramInput inputSource, PersistedListCollection<ProgramInputData> logDestiation)
        {
            _inputSource = inputSource;
            _recordedInputs = logDestiation;
        }

        public bool IsKeyAvailable() => _inputSource.IsKeyAvailable();

        public ProgramInputData ReadKey(ConsoleKey[] allowedKeys)
        {
            var key = _inputSource.ReadKey(allowedKeys);
            while (!allowedKeys.Contains((ConsoleKey)key.Key)) key = _inputSource.ReadKey();
            _recordedInputs.Add(key);
            return key;
        }

        public ProgramInputData ReadKey()
        {
            var key = _inputSource.ReadKey();
            _recordedInputs.Add(key);
            return key;
        }
    }
}