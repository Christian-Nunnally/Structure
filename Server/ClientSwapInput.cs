using Structure.IO;
using Structure.IO.Input;
using Structure.IO.Output;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Structure.Server
{
    public class ClientSwapInput : IProgramInput
    {
        public bool ShouldSwapClients;
        public bool IsReadyToSwapClients;

        public StructureIO IO { get; set; }
        public IProgramOutput OutputToSwapTo { get; }
        public INewsPrinter NewsPrinter { get; }

        public ClientSwapInput(StructureIO io, IProgramOutput outputToSwapTo, INewsPrinter newsPrinter)
        {
            IO = io;
            OutputToSwapTo = outputToSwapTo;
            NewsPrinter = newsPrinter;
        }

        public bool IsKeyAvailable()
        {
            IsReadyToSwapClients = true;
            while (!ShouldSwapClients)
            {
                Thread.Sleep(500);
            }
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

        public ProgramInputData ReadKey()
        {
            throw new InvalidOperationException();
        }

        public void RemoveLastReadKey()
        {
        }
    }
}
