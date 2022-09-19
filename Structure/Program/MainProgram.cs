﻿using Structur.IO;
using Structur.IO.Input;
using Structur.IO.Output;
using Structur.Modules;
using Structur.Server;
using System;

namespace Structur.Program
{
    public static class MainProgram
    {
        public static void Main()
        {
            var ioc = CreateIoCContainer();
            RunStructure(ioc, new ExitToken());
        }

        public static void RunStructure(StructureIoC ioc, ExitToken exitToken)
        {
            try
            {
                var newsPrinter = ioc.Get<INewsPrinter>();
                var io = new StructureIO(ioc);
                var consoleOutput = ioc.Get<IProgramOutput>();
                var consoleInput = ioc.Get<IProgramInput>();
                var noopOutput = ioc.Get<NoOpOutput>();
                var settings = ioc.Get<Settings>();
                var enableClient = settings.EnableClient;
                var enableServer = settings.EnableWebServer;
                var enableDebugging = settings.EnableDebugging;
                var startingModules = StartingModules.Create();
                var program = new StructureProgram(ioc, io, startingModules);

                if (enableClient)
                {
                    var clientSwapInput = new OutputSwapInput(io, consoleOutput, newsPrinter);
                    var chainedInput = new ChainedInput();
                    chainedInput.AddInput(clientSwapInput);
                    chainedInput.AddInput(consoleInput);
                    io.ProgramInput = WrapInputWithStructureInput(io, enableDebugging, chainedInput, noopOutput, newsPrinter);
                    io.ProgramOutput = noopOutput;

                    var clientIO = new StructureIO(ioc);
                    clientIO.ProgramInput = consoleInput;
                    clientIO.ProgramOutput = consoleOutput;
                    var client = new Client(clientIO, settings.ServerHostname, clientSwapInput, program, io);
                    client.RunAsync(exitToken).Wait();
                }
                else
                {
                    IProgramInput mainProgramInput = consoleInput;
                    if (enableServer)
                    {
                        mainProgramInput = EnableWebServerAndRouteConsoleOrServerKeysToInput(io, settings.Hostname, mainProgramInput);
                    }
                    io.ProgramInput = WrapInputWithStructureInput(io, enableDebugging, mainProgramInput, consoleOutput, newsPrinter);
                    io.ProgramOutput = consoleOutput;
                    program.Run(exitToken);
                }
            }
            catch (ArgumentException e)
            {
                var errorOutput = ioc.Get<IProgramOutput>();
                errorOutput.WriteLine($"[ERROR] {e.Message}");
            }
            catch (UriFormatException e)
            {
                var errorOutput = ioc.Get<IProgramOutput>();
                errorOutput.WriteLine($"[ERROR] {e.Message}");
            }
        }

        private static IProgramInput WrapInputWithStructureInput(StructureIO io, bool enableDebugging, IProgramInput mainProgramInput, IProgramOutput outputToSwitchToAfterLoading, INewsPrinter newsPrinter)
        {
            return enableDebugging
                ? new DevelopmentStructureInput(io, mainProgramInput, outputToSwitchToAfterLoading, newsPrinter)
                : new StructureInput(io, mainProgramInput, outputToSwitchToAfterLoading, newsPrinter);
        }

        private static MultiInput EnableWebServerAndRouteConsoleOrServerKeysToInput(StructureIO io, string serverHostname, IProgramInput input)
        {
            var webServerInput = new QueuedInput();
            EnableWebServer(io, webServerInput, serverHostname);
            var multiInput = new MultiInput();
            multiInput.AddInput(webServerInput);
            multiInput.AddInput(input);
            return multiInput;
        }

        private static void EnableWebServer(StructureIO io, QueuedInput input, string uri)
        {
            var controller = new IOController(io, input);
            var server = new Server.StructureServer(uri, controller);
            server.RunInNewThread();
        }

        public static StructureIoC CreateIoCContainer()
        {
            var ioc = new StructureIoC();
            ioc.Register<Hotkey>();
            ioc.Register<CurrentTime>();
            ioc.Register<StructureData>();
            ioc.Register<NoOpOutput>();
            var newsPrinter = new NewsPrinter();
            var delayer = new BackgroundDelay();
            var staleOutputClearer = new StaleOutputClearer();
            ioc.Register<INewsPrinter>(() => newsPrinter);
            ioc.Register<IBackgroundProcess>(() => staleOutputClearer);
            ioc.Register<IBackgroundProcess>(() => delayer, "delayer");
            ioc.Register<IBackgroundProcess>(() => newsPrinter);
            ioc.Register<Settings>(() => Settings.ReadSettings());
            ioc.Register<IProgramOutput>(typeof(ConsoleOutput));
            ioc.Register<IProgramInput>(typeof(ConsoleInput));
            return ioc;
        }
    }
}