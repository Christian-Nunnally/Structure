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
            var io = new StructureIO(ioc);
            var settings = Settings.ReadSettings();

            IProgramInput mainProgramInput = settings.EnableWebServer
                ? EnableWebServerAndRouteConsoleOrServerKeysToInput(io, settings.ServerHostname)
                : (IProgramInput)new ConsoleInput();

            var clientSwapInput = new ClientSwapInput(io, new ConsoleOutput(), ioc.Get<INewsPrinter>());
            if (settings.EnableClient)
            {
                var chainedInput = new ChainedInput();
                chainedInput.AddInput(clientSwapInput);
                chainedInput.AddInput(new ConsoleInput());
                mainProgramInput = chainedInput;
            }

            var startingModules = StartingModules.Create();
            var program = new StructureProgram(ioc, io, startingModules);

            if (settings.EnableDebugging) io.ProgramInput = new DevelopmentStructureInput(io, mainProgramInput, new ConsoleOutput(), ioc.Get<INewsPrinter>(), true);
            else if (settings.EnableClient) io.ProgramInput = new StructureInput(io, mainProgramInput, new NoOpOutput(), ioc.Get<INewsPrinter>());
            else io.ProgramInput = new StructureInput(io, mainProgramInput, new ConsoleOutput(), ioc.Get<INewsPrinter>());

            io.ProgramOutput = new ConsoleOutput();

            if (settings.EnableClient)
            {
                io.ProgramOutput = new NoOpOutput();
            }

            if (settings.EnableClient)
            {
                var clientIO = new StructureIO(ioc);
                clientIO.ProgramInput = new ConsoleInput();
                clientIO.ProgramOutput = new ConsoleOutput();
                var client = new Client(clientIO, settings.ServerHostname, clientSwapInput, program);
                client.RunAsync().Wait();
                return;
            }
            else
            {
                program.Run();
            }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The webserver is meant to stay running for the lifespan of the application.")]
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