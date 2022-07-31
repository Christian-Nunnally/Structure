using System.Collections.Generic;

namespace Structur.IO.Input
{
    class QueuedInput : IProgramInput
    {
        private readonly Queue<ProgramInputData> _inputQueue = new();

        public bool IsInputAvailable() => _inputQueue.Count > 0;

        public ProgramInputData ReadInput() => _inputQueue.Dequeue();

        public void EnqueueInput(ProgramInputData key) => _inputQueue.Enqueue(key);

        public void RemoveLastInput() { }
    }
}
