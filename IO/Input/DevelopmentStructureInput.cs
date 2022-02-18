using Structure.IO;

namespace Structure.IO.Input
{
    public class DevelopmentStructureInput : StructureInput
    {
        public DevelopmentStructureInput(StructureIO io, INewsPrinter newsPrinter, bool loadSave) : base(io, newsPrinter)
        {
            if (loadSave)
            {
                InitializeNewInputFromSavedSessions(io, newsPrinter);
            }
            else
            {
                InputSource = new ChainedInput();
            }
            InputSource.AddInput(new ConsoleInput());
        }
    }
}