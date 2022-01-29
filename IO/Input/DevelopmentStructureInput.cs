using Structure.IO;

namespace Structure.Code
{
    public class DevelopmentStructureInput : StructureInput
    {
        public DevelopmentStructureInput(StructureIO io, NewsPrinter newsPrinter, bool loadSave) : base(io, newsPrinter)
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