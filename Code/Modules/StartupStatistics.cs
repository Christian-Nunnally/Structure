using System.IO;
using System.Linq;

namespace Structure
{
    public class StartupStatistics : Module
    {
        public override void Enable()
        {
            var currentSaveSize = Directory.GetFiles(FileIO.SavePath).Select(x => new FileInfo(x).Length).Sum();
            currentSaveSize /= 1024;
            var unit = "kb";
            if (currentSaveSize > 1024)
            {
                currentSaveSize /= 1024;
                unit = "mb";
            }
            IO.News($"Data = {currentSaveSize} {unit}");
        }
    }
}