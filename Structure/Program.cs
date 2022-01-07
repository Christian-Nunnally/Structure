using Structure.Code;

namespace Structure
{
    public static class Program
    {
        public static void Main()
        {
            var hotkey = new Hotkey();
            var io = new StructureIO(hotkey);
            io.ProgramInput = new StructureInput(io);
            io.ProgramOutput = new ConsoleOutput();
            var program = new StructureProgram(io);
            program.Run();
        }
    }
}