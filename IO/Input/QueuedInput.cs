using System.Collections.Generic;

namespace Structure.IO.Input
{
    class QueuedInput : IProgramInput
    {
        private readonly Queue<ProgramInputData> _inputQueue = new Queue<ProgramInputData>();

        public bool IsKeyAvailable() => _inputQueue.Count > 0;

        public ProgramInputData ReadKey() => _inputQueue.Dequeue();

        public void EnqueueKey(ProgramInputData key) => _inputQueue.Enqueue(key);

        public void RemoveLastReadKey() { }
    }
}
