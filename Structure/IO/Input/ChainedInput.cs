using System;
using System.Collections.Generic;
using System.Linq;

namespace Structur.IO.Input
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

        public bool IsInputAvailable()
        {
            if (_currentInput is object && _currentInput.IsInputAvailable()) return true;
            LoadNextInput();
            return _currentInput.IsInputAvailable();
        }

        public ProgramInputData ReadInput()
        {
            if (_currentInput is null) LoadNextInput();
            if (!_currentInput.IsInputAvailable()) LoadNextInput();
            return _currentInput.ReadInput();
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

        public void RemoveLastInput() => _currentInput?.RemoveLastInput();
    }
}