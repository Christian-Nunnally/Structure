using Structure.Code;

namespace Structure
{
    public static class Program
    {
        public static void Main()
        {
            var io = new StructureIO();
            var input = new StructureInput();
            var output = new ConsoleOutput();
            input.InitializeStructureInput(io);
            io.SetInput(input);
            io.SetOutput(output);

            var program = new StructureProgram(io, new Hotkey());
            program.Run();
        }
    }
}