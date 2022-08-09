using Structur.IO;
using Structur.IO.Input;
using Structur.IO.Output;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Structur.Server
{
    public class OutputSwapInput : IProgramInput
    {
        private bool _isReadyToSwapOutputs;
        private bool _initiateOutputSwap;
        private readonly QueuedInput _inputQueue = new();

        public StructureIO IO { get; set; }
        public IProgramOutput OutputToSwapTo { get; }
        public INewsPrinter NewsPrinter { get; }

        public bool IsReadyToSwapOutputs() => _isReadyToSwapOutputs;

        public void InitiateOutputSwap() => _initiateOutputSwap = true;

        public OutputSwapInput(StructureIO io, IProgramOutput outputToSwapTo, INewsPrinter newsPrinter)
        {
            IO = io;
            OutputToSwapTo = outputToSwapTo;
            NewsPrinter = newsPrinter;
        }

        public bool IsInternalInputAvailable() => _inputQueue.IsInputAvailable();

        public bool IsInputAvailable()
        {
            if (_inputQueue.IsInputAvailable()) return true;
            _isReadyToSwapOutputs = true;
            while (!_initiateOutputSwap)
            {
                if (_inputQueue.IsInputAvailable()) return true; 
                Thread.Sleep(333);
            }
            SwapOutput();
            return false;
        }

        private void SwapOutput()
        {
            IO.SkipUnescesscaryOperations = false;
            IO.ProgramOutput = OutputToSwapTo;
            var buffer = IO.CurrentBuffer.ToString();
            IO.ClearBuffer();
            IO.WriteNoLine(buffer);
            IO.ClearStaleOutput();
        }

        public ProgramInputData ReadInput() => _inputQueue.ReadInput();

        public void RemoveLastInput()
        {
        }

        internal void EnqueueInput(ProgramInputData input)
        {
            _inputQueue.EnqueueInput(input);
        }
    }
}
