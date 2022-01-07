using Structure.Code;

namespace Structure
{
    public static class Program
    {
        public static void Main()
        {
            var io = new StructureIO();
            io.ProgramInput = new StructureInput(io);
            io.ProgramOutput = new ConsoleOutput();

            var hotkey = new Hotkey();
            var program = new StructureProgram(io, hotkey);
            program.Run();
        }
    }
}