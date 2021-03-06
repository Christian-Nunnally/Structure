using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.IO.Input
{
    public class ChainedInput : IProgramInput
    {
        private readonly Queue<object> _chainedInputs = new Queue<object>();
        private IProgramInput _currentInput;

        public void AddInput(IProgramInput input)
        {
            _chainedInputs.Enqueue(input);
        }

        public void AddAction(Action action)
        {
            _chainedInputs.Enqueue(action);
        }

        public bool IsKeyAvailable()
        {
            if (_currentInput is object && _currentInput.IsKeyAvailable()) return true;
            LoadNextInput();
            return _currentInput.IsKeyAvailable();
        }

        public ProgramInputData ReadKey()
        {
            if (!_currentInput.IsKeyAvailable()) LoadNextInput();
            return _currentInput.ReadKey();
        }

        private void LoadNextInput()
        {
            if (!_chainedInputs.Any()) return;
            var nextInput = _chainedInputs.Dequeue();
            while (nextInput is Action action)
            {
                action();
                if (!_chainedInputs.Any()) break;
                nextInput = _chainedInputs.Dequeue();
            }
            if (nextInput is IProgramInput input) _currentInput = input;
        }

        public void RemoveLastReadKey() => _currentInput?.RemoveLastReadKey();
    }
}