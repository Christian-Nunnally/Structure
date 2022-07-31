using Structur.IO;
using Structur.IO.Input;
using Structur.IO.Output;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Structur.Server
{
    public class ClientSwapInput : IProgramInput
    {
        private bool _isReadyToSwapClients;
        private bool _swapClients;

        public StructureIO IO { get; set; }
        public IProgramOutput OutputToSwapTo { get; }
        public INewsPrinter NewsPrinter { get; }

        public bool IsInputLoaded() => _isReadyToSwapClients;

        public void SwapClients() => _swapClients = true;

        public ClientSwapInput(StructureIO io, IProgramOutput outputToSwapTo, INewsPrinter newsPrinter)
        {
            IO = io;
            OutputToSwapTo = outputToSwapTo;
            NewsPrinter = newsPrinter;
        }

        public bool IsInputAvailable()
        {
            _isReadyToSwapClients = true;
            while (!_swapClients) Thread.Sleep(500);
            SwapClient();
            return false;
        }

        private void SwapClient()
        {
            IO.ProgramOutput = OutputToSwapTo;
            var currentBuffer = IO.CurrentBuffer.ToString();
            IO.ClearBuffer();
            IO.Write(currentBuffer);
            IO.ClearStaleOutput();
            while (NewsPrinter.DoProcess(IO));
            IO.ProcessAllBackgroundWork();
        }

        public ProgramInputData ReadInput()
        {
            throw new InvalidOperationException();
        }

        public void RemoveLastInput()
        {
        }
    }
}
