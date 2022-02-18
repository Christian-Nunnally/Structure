using Structure.IO.Output;

namespace Structure.IO
{
    public class NoOpNewsPrinter : INewsPrinter
    {
        public bool PrintNews(IProgramOutput programOutput) => false;

        public void ClearNews() { }

        public void EnqueueNews(string news) { }

        public bool DoProcess(StructureIO io) => false;
    }
}
