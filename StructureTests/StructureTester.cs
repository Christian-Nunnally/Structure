using Microsoft.VisualStudio.TestTools.UnitTesting;
using Structur.IO.Input;
using Structur.IO.Output;
using Structur.Program;
using Structur.Program.Utilities;
using StructureTests.Utilities;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace StructureTests
{
    public class StructureTester
    {
        private ExitToken ExitToken { get; set; }
        
        public Settings Settings { get; set; }

        public TextOutput TestOutput { get; set; }

        protected string Output => TestOutput.Read();

        public StructureTester()
        {
            ExitToken = new ExitToken();
            TestOutput = new TextOutput();
            Settings = new Settings() { EnableDebugging = true };
        }

        public void Run(params ConsoleKeyInfo[] inputData)
        {
            Run(ExitToken.ExitingToken, inputData);
        }

        public DisposableAction RunWithoutStopping(params ConsoleKeyInfo[] inputData)
        {
            var exitToken = new ExitToken();
            var isRunning = true;
            new Thread(() => 
            {
                Run(exitToken, inputData); 
                isRunning = false; 
            }).Start();
            return new DisposableAction(() =>
            {
                exitToken.Exit = true;
                var count = 0;
                while (isRunning) 
                {
                    if (count++ > 1000) throw new InvalidOperationException("Timeout: Server did not stop running.");
                    Thread.Sleep(10);
                };
            });
        }

        private void Run(ExitToken exitToken, ConsoleKeyInfo[] inputData)
        {
            var input = CreateTestInput(exitToken, TestOutput, inputData);
            Run(input, TestOutput, Settings);
            TestOutput.Clear();
        }

        public void Run(IProgramInput input, TextOutput output, Settings settings)
        {
            var ioc = CreateTestIoCContainer(input, output, settings);
            MainProgram.RunStructure(ioc, ExitToken);
        }

        private static StructureIoC CreateTestIoCContainer(IProgramInput input, TextOutput output, Settings settings)
        {
            var ioc = MainProgram.CreateIoCContainer();
            ioc.Register<Settings>(() => settings);
            ioc.Register<IProgramOutput>(() => output);
            ioc.Register<IProgramInput>(() => input);
            return ioc;
        }

        private IProgramInput CreateTestInput(ExitToken readyExitToken, TextOutput testOutput, params ConsoleKeyInfo[] inputData)
        {
            var programInputData = inputData.Select(x => new ProgramInputData(x, DateTime.Now));
            var input = new ChainedInput();
            input.AddInput(new PredeterminedInput(programInputData));
            input.AddInput(new ExitingProgramInput(ExitToken, readyExitToken, testOutput));
            return input;
        }

        public void Contains(string substring)
        {
            var allOutput = TestOutput.Screens.Aggregate("", (s, s2) => s + "\n\n---\n\n" + s2);
            if (!allOutput.Contains(substring)) Assert.Fail($"'{substring}' not found in:\n\n{allOutput}");
        }

        public void NotContains(string substring)
        {
            var allOutput = TestOutput.Screens.Aggregate("", (s, s2) => s + "\n\n---\n\n" + s2);
            if (allOutput.Contains(substring)) Assert.Fail($"'{substring}' found in:\n\n{allOutput}");
        }
    }
}