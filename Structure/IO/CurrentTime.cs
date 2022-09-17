using System;

namespace Structur.IO
{
    public class CurrentTime
    {
        private DateTime _artificialCurrentTime;
        private bool _useArtificalTime;

        public DateTime Time => _useArtificalTime
            ? _artificialCurrentTime
            : DateTime.Now;

        public void SetArtificialTime(DateTime time)
        {
            _artificialCurrentTime = time;
            _useArtificalTime = true;
        }

        public void SetToRealTime()
        {
            _useArtificalTime = false;
        }
    }
}