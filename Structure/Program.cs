using Structure.IO;
using Structure.IO.Input;
using Structure.IO.Output;
using Structure.Modules;
using Structure.Server;
using System.IO;

namespace Structure.Structure
{
    public static class Program
    {
        public static void Main()
        {
            var ioc = CreateIoCContainer();
            var io = new StructureIO(ioc);

            IProgramInput input = new ConsoleInput();

            var uri = "http://localhost:9696/";
            if (DoesFileExist("server"))
            {
                input = EnableWebServer(io, input, uri);
            }
            if (DoesFileExist("client"))
            {
                io.ProgramInput = new ConsoleInput();
                io.ProgramOutput = new ConsoleOutput();
                var client = new Client(io, File.ReadAllText("client.structure"));
                client.RunAsync().Wait();
                return;
            }

            var startingModules = StartingModules.Create();
            var program = new StructureProgram(ioc, io, startingModules);
            io.ProgramInput = new StructureInput(io, input, ioc.Get<INewsPrinter>());
            if (DoesFileExist("debug")) io.ProgramInput = new DevelopmentStructureInput(io, input, ioc.Get<INewsPrinter>(), true);
            io.ProgramOutput = new ConsoleOutput();
            program.Run();
        }

        private static bool DoesFileExist(string name) => File.Exists($"{name}.structure");

        private static IProgramInput EnableWebServer(StructureIO io, IProgramInput input, string uri)
        {
            var queuedInput = new QueuedInput();
            var controller = new Controller(io, queuedInput);
            var server = new Server.Server(uri, controller);
            server.RunInNewThread();

            var multiInput = new MultiInput();
            multiInput.AddInput(queuedInput);
            multiInput.AddInput(input);
            return multiInput;
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