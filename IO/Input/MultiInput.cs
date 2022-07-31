using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Structur.IO.Input
{
    public class MultiInput : IProgramInput
    {
        private readonly List<IProgramInput> _inputs = new List<IProgramInput>();
        private IProgramInput _currentInput;

        public void AddInput(IProgramInput input)
        {
            _inputs.Add(input);
        }

        public bool IsInputAvailable()
        {
            return _inputs.Any(i => i.IsInputAvailable());
        }

        public ProgramInputData ReadInput()
        {
            while (true)
            {
                foreach (var input in _inputs)
                {
                    if (input.IsInputAvailable())
                    {
                        _currentInput = input;
                        return _currentInput.ReadInput();
                    }
                }
                Thread.Sleep(33);
            }
        }

        public void RemoveLastInput() => _currentInput?.RemoveLastInput();
    }
}