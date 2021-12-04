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
            _numberOfInputs = inputData.Count();
            _enumerator = inputData.GetEnumerator();
        }

        public bool IsKeyAvailable() => _currentInputIndex < _numberOfInputs;

        public ConsoleKeyInfo ReadKey()
        {
            _currentInputIndex++;
            _enumerator.MoveNext();
            // TODO: return the whole input object and set the time elsewhere.
            CurrentTime.SetToArtificialTime(_enumerator.Current.Time);
            return _enumerator.Current.GetKeyInfo();
        }
    }
}