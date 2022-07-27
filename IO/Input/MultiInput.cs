using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Structure.IO.Input
{
    public class MultiInput : IProgramInput
    {
        private readonly List<IProgramInput> _inputs = new List<IProgramInput>();
        private IProgramInput _currentInput;

        public void AddInput(IProgramInput input)
        {
            _inputs.Add(input);
        }

        public bool IsKeyAvailable()
        {
            return _inputs.Any(i => i.IsKeyAvailable());
        }

        public ProgramInputData ReadKey()
        {
            while (true)
            {
                foreach (var input in _inputs)
                {
                    if (input.IsKeyAvailable())
                    {
                        _currentInput = input;
                        return _currentInput.ReadKey();
                    }
                }
                Thread.Sleep(33);
            }
        }

        public void RemoveLastReadKey() => _currentInput?.RemoveLastReadKey();
    }
}