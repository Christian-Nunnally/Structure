using System.Threading;

namespace Structure.IO
{
    public class BackgroundDelay : IBackgroundProcess
    {
        public bool DoProcess(StructureIO io)
        {
            Thread.Sleep(10);
            return false;
        }
    }
}
