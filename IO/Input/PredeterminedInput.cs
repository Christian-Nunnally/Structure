using System.Collections.Generic;
using System.Linq;

namespace Structure.IO.Input
{
    public class PredeterminedInput : IProgramInput
    {
        private readonly IEnumerable<ProgramInputData> _inputData;
        private IEnumerator<ProgramInputData> _cachedEnumerator;
        private int? _cachedNumberOfInputs;
        private int _currentInputIndex;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "This is bugged and wont cast to an int without the dead code.")]
        private int NumberOfInputs => _cachedNumberOfInputs ?? (_cachedNumberOfInputs = _inputData.Count()) ?? 0;

        private IEnumerator<ProgramInputData> Enumerator => _cachedEnumerator ?? (_cachedEnumerator = _inputData.GetEnumerator());

        public PredeterminedInput(IEnumerable<ProgramInputData> inputData)
        {
            if (inputData is null) return;
            _inputData = inputData;
        }

        public bool IsKeyAvailable() => _currentInputIndex < NumberOfInputs;

        public ProgramInputData ReadKey()
        {
            _currentInputIndex++;
            Enumerator.MoveNext();
            return Enumerator.Current;
        }

        public void RemoveLastReadKey()
        {
        }
    }
}