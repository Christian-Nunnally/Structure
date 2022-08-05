using Structur.IO;
using Structur.IO.Input;
using Structur.IO.Output;
using Structur.Modules;
using Structur.Server;

namespace Structur.Program
{
    public static class MainProgram
    {
        public static void Main()
        {
            var ioc = CreateIoCContainer();
            var newsPrinter = ioc.Get<INewsPrinter>();
            var io = new StructureIO(ioc);
            var consoleOutput = new ConsoleOutput();
            var consoleInput = new ConsoleInput();
            var noopOutput = new NoOpOutput();
            var settings = Settings.ReadSettings();
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
                client.RunAsync().Wait();
            }
            else
            {
                IProgramInput mainProgramInput = consoleInput;
                if (enableServer)
                {
                    mainProgramInput = EnableWebServerAndRouteConsoleOrServerKeysToInput(io, settings.Hostname);
                }
                io.ProgramInput = WrapInputWithStructureInput(io, enableDebugging, mainProgramInput, consoleOutput, newsPrinter);
                io.ProgramOutput = consoleOutput;
                program.Run();
            }
        }

        private static IProgramInput WrapInputWithStructureInput(StructureIO io, bool enableDebugging, IProgramInput mainProgramInput, IProgramOutput outputToSwitchToAfterLoading, INewsPrinter newsPrinter)
        {
            return enableDebugging
                ? new DevelopmentStructureInput(io, mainProgramInput, outputToSwitchToAfterLoading, newsPrinter)
                : new StructureInput(io, mainProgramInput, outputToSwitchToAfterLoading, newsPrinter);
        }

        private static MultiInput EnableWebServerAndRouteConsoleOrServerKeysToInput(StructureIO io, string serverHostname)
        {
            var webServerInput = new QueuedInput();
            EnableWebServer(io, webServerInput, serverHostname);
            var multiInput = new MultiInput();
            multiInput.AddInput(webServerInput);
            multiInput.AddInput(new ConsoleInput());
            return multiInput;
        }

        private static void EnableWebServer(StructureIO io, QueuedInput input, string uri)
        {
            var controller = new IOController(io, input);
            var server = new Server.Server(uri, controller);
            server.RunInNewThread();
        }

        private static StructureIoC CreateIoCContainer()
        {
            var ioc = new StructureIoC();
            ioc.Register<Hotkey>();
            ioc.Register<CurrentTime>();
            ioc.Register<StructureData>();
            var newsPrinter = new NewsPrinter();
            var delayer = new BackgroundDelay();
            var staleOutputClearer = new StaleOutputClearer();
            ioc.Register<INewsPrinter>(() => newsPrinter);
            ioc.Register<IBackgroundProcess>(() => staleOutputClearer);
            ioc.Register<IBackgroundProcess>(() => delayer);
            ioc.Register<IBackgroundProcess>(() => newsPrinter);
            return ioc;
        }
    }
}