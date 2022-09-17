using Structur.IO.Output;

namespace Structur.IO
{
    public interface INewsPrinter : IBackgroundProcess
    {
        bool PrintNews(IProgramOutput programOutput);

        void EnqueueNews(string news);

        void Disable();

        void Enable();
    }
}