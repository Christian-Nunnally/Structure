using System;
using System.Collections.Generic;
using System.Linq;

namespace Structure.Code
{
    public class PredeterminedInput : IProgramInput
    {
        private readonly IEnumerator<ProgramInputData> _enumerator;
        private readonly int _numberOfInputs;
        private int _currentInputIndex;

        public PredeterminedInput(IEnumerable<ProgramInputData> inputData)
        {
            if (inputData is null) return;
            _numberOfInputs = inputData.Count();
            _enumerator = inputData.GetEnumerator();
        }

        public bool IsKeyAvailable() => _currentInputIndex < _numberOfInputs;

        public ProgramInputData ReadKey()
        {
            _currentInputIndex++;
            _enumerator.MoveNext();
            return _enumerator.Current;
        }
    }
}