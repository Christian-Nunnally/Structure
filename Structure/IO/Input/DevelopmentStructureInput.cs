using Structur.IO;
using Structur.IO.Output;

namespace Structur.IO.Input
{
    public class DevelopmentStructureInput : StructureInput
    {
        public DevelopmentStructureInput(StructureIO io, IProgramInput sourceInput, IProgramOutput sourceOutput, INewsPrinter newsPrinter) : base(io, sourceInput, sourceOutput, newsPrinter)
        {
            InitializeNewInputFromSavedSessions(io, newsPrinter);
            InputSource.AddInput(sourceInput);
        }
    }
}