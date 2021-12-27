using System;

namespace Structure
{
    internal static class CurrentTime
    {
        private static DateTime _artificialCurrentTime;
        private static bool _useArtificalTime;

        public static DateTime GetCurrentTime()
        {
            return _useArtificalTime
                ? _artificialCurrentTime
                : DateTime.Now;
        }

        public static void SetArtificialTime(DateTime time)
        {
            _artificialCurrentTime = time;
            _useArtificalTime = true;
        }

        public static void SetToRealTime()
        {
            _useArtificalTime = false;
        }
    }
}