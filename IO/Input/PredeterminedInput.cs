using System.Collections.Generic;
using System.Linq;

namespace Structur.IO.Input
{
    public class PredeterminedInput : IProgramInput
    {
        private readonly IEnumerable<ProgramInputData> _inputData;
        private IEnumerator<ProgramInputData> _cachedEnumerator;
        private int _currentInputIndex;

        public int NumberOfInputs => _inputData.Count();

        private IEnumerator<ProgramInputData> Enumerator => _cachedEnumerator ??= _inputData.GetEnumerator();

        public PredeterminedInput(IEnumerable<ProgramInputData> inputData)
        {
            if (inputData is null) return;
            _inputData = inputData;
        }

        public bool IsInputAvailable() => _currentInputIndex < NumberOfInputs;

        public ProgramInputData ReadInput()
        {
            _currentInputIndex++;
            Enumerator.MoveNext();
            return Enumerator.Current;
        }

        public void RemoveLastInput()
        {
        }
    }
}