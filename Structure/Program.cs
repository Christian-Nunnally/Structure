using Structure.IO;
using Structure.IO.Input;
using Structure.IO.Output;
using Structure.Modules;

namespace Structure.Structure
{
    public static class Program
    {
        public static void Main()
        {
            var ioc = CreateIoCContainer();

            var io = new StructureIO(ioc);
            io.ModifierKeyAction = ioc.Get<Hotkey>().Execute;
            var startingModules = StartingModules.Create();
            var program = new StructureProgram(ioc, io, startingModules);
            io.ProgramInput = new StructureInput(io, ioc.Get<INewsPrinter>());
            io.ProgramInput = new DevelopmentStructureInput(io, ioc.Get<INewsPrinter>(), true);
            io.ProgramOutput = new ConsoleOutput();
            program.Run();
        }

        private static StructureIoC CreateIoCContainer()
        {
            var ioc = new StructureIoC();
            ioc.Register<Hotkey>();
            ioc.Register<CurrentTime>();
            ioc.Register<StructureData>();
            var newsPrinter = new NewsPrinter();
            ioc.Register<INewsPrinter>(() => newsPrinter);
            ioc.Register<IBackgroundProcess>(() => newsPrinter);
            return ioc;
        }
    }
}