using Structure.Code;
using Structure.IO;

namespace Structure
{
    public static class Program
    {
        public static void Main()
        {
            var hotkey = new Hotkey();
            var newsPrinter = new NewsPrinter();
            var io = new StructureIO(hotkey, newsPrinter);
            io.ProgramInput = new StructureInput(io, newsPrinter);
            io.ProgramOutput = new ConsoleOutput();
            var program = new StructureProgram(io);
            program.Run();
        }
    }
}