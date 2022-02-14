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
            var hotkey = new Hotkey();
            var newsPrinter = new NewsPrinter();
            var io = new StructureIO(hotkey, newsPrinter);
            io.ProgramInput = new StructureInput(io, newsPrinter);
            //io.ProgramInput = new DevelopmentStructureInput(io, newsPrinter, true);
            io.ProgramOutput = new ConsoleOutput();
            var data = new StructureData();
            var startingModules = StartingModules.Create();
            var program = new StructureProgram(io, data, startingModules);
            program.Run();
        }
    }
}