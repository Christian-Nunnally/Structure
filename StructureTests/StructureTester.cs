using Microsoft.VisualStudio.TestTools.UnitTesting;
using Structur.IO;
using Structur.IO.Input;
using Structur.IO.Output;
using Structur.Program;
using Structur.Program.Utilities;
using StructureTests.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace StructureTests
{
    public class StructureTester
    {
        private bool _infoPrinted = false;
        private bool _isRunning;

        private ExitToken ExitToken { get; set; }
        
        public Settings Settings { get; set; }

        public TextOutput TestOutput { get; set; }

        public IList<string> Screens { get; } = new List<string>();

        protected string Output => TestOutput.Read();

        public List<ConsoleKeyInfo> _inputQueue = new();

        public StructureTester()
        {
            ExitToken = new ExitToken();
            TestOutput = new TextOutput();
            Settings = new Settings() { EnableDebugging = true };
        }

        public void Queue(params ConsoleKeyInfo[] inputData)
        {
            _inputQueue.AddRange(inputData);
        }

        public void Run(params ConsoleKeyInfo[] inputData)
        {
            var timeout = 1000000;
            RunWithoutStopping(inputData);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (_isRunning && stopwatch.ElapsedMilliseconds < timeout);
            return;
        }

        public DisposableAction RunWithoutStopping(params ConsoleKeyInfo[] inputData)
        {
            Queue(inputData);
            var exitToken = new ExitToken();
            _isRunning = true;
            new Thread(() => 
            {
                Run(exitToken); 
                _isRunning = false; 
            }).Start();
            return new DisposableAction(() =>
            {
                exitToken.Exit = true;
                var count = 0;
                while (_isRunning) 
                {
                    if (count++ > 100) throw new InvalidOperationException("Timeout: Server did not stop running.");
                    Thread.Sleep(10);
                };
            });
        }

        private void Run(ExitToken exitToken)
        {
            var input = CreateTestInput(exitToken);
            var ioc = CreateTestIoCContainer(input, TestOutput, Settings);
            MainProgram.RunStructure(ioc, ExitToken);
            Screens.Add(TestOutput.Read());
        }


        private static StructureIoC CreateTestIoCContainer(IProgramInput input, TextOutput output, Settings settings)
        {
            var ioc = MainProgram.CreateIoCContainer();
            ioc.Register<Settings>(() => settings);
            ioc.Register<IProgramOutput>(() => output);
            ioc.Register<IProgramInput>(() => input);
            ioc.Unregister<IBackgroundProcess>("delayer");
            return ioc;
        }

        private IProgramInput CreateTestInput(ExitToken readyExitToken)
        {
            var programInputData = _inputQueue.Select(x => new ProgramInputData(x, DateTime.Now));
            var input = new ChainedInput();
            input.AddAction(RecordScreen);
            foreach (var inputData in programInputData)
            {
                input.AddInput(new PredeterminedInput(inputData));
                input.AddAction(() => MainProgram.MainIO.ProcessAllBackgroundWork());
                input.AddAction(() => RecordScreen(inputData));

            }
            input.AddInput(new ExitingProgramInput(ExitToken, readyExitToken));
            return input;
        }

        public void RecordScreen()
        {
            Screens.Add(TestOutput.Read());
        }

        public void RecordScreen(ProgramInputData input)
        {
            var modifiers = (ConsoleModifiers)input.Modifiers;
            var hasCtrl = modifiers.HasFlag(ConsoleModifiers.Control);
            var hasShift = modifiers.HasFlag(ConsoleModifiers.Shift);
            var hasAlt = modifiers.HasFlag(ConsoleModifiers.Alt);
            Screens.Add($"Input <- {(hasShift ? "shift + " : "")}{(hasAlt ? "alt + " : "")}{(hasCtrl ? "ctrl + " : "")}{input.GetKeyInfo().Key}");
            RecordScreen();
        }

        public void Contains(string substring)
        {
            var allOutput = Screens.Aggregate("", (s, s2) => s + "\n\n---\n\n" + s2);
            PrintInfo();
            if (!allOutput.Contains(substring))
            {
                TestOutput.WriteDebugStrings();
                Assert.Fail($"'{substring}' not found in:\n\n{allOutput}");
            }
        }

        public void NotContains(string substring)
        {
            var allOutput = Screens.Aggregate("", (s, s2) => s + "\n\n---\n\n" + s2);
            PrintInfo();
            if (allOutput.Contains(substring))
            {
                TestOutput.WriteDebugStrings();
                Assert.Fail($"'{substring}' found in:\n\n{allOutput}");
            }
        }

        public void PrintInfo()
        {
            if (_infoPrinted) return;
            _infoPrinted = true;
            Console.WriteLine("Structure test");
            foreach (var screen in Screens)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine(screen);
            }
            Console.WriteLine("----------------------------------------");
        }
    }
}