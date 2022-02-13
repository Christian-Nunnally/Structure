using Structure.IO;

namespace Structure.IO.Input
{
    public class ExitingStructureInput : StructureInput
    {
        public ExitingStructureInput(StructureIO io, NewsPrinter newsPrinter) : base(io, newsPrinter)
        {
            InitializeNewInputFromSavedSessions(io, newsPrinter);
        }
    }
}