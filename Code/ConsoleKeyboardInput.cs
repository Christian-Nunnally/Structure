using System;
using System.Linq;

namespace Structure.Code
{
    public class ConsoleKeyboardInput : IKeyboardInput
    {
        private int _loadedSession;
        private int _currentIndex;
        private PersistedList<KeyboardInputData> _consoleKeyInfos;
        private bool _hasLoaded = false;

        public bool IsKeyAvailable()
        {
            if (_hasLoaded) return Console.KeyAvailable;
            return true;
        }

        public ConsoleKeyInfo ReadKey()
        {
            if (_hasLoaded) return ReadAndRecordKey();

            IO.SupressConsoleCalls = true;
            if (ShouldLoadNextSession())
            {
                LoadNextSession();
                if (IsSessionEmpty())
                {
                    _hasLoaded = true;
                    IO.SupressConsoleCalls = false;
                    CurrentTime.SetToRealTime();
                    IO.Refresh();
                    return ReadAndRecordKey();
                }
            }

            return ReadNextKeyFromLoadedSession();
        }

        private ConsoleKeyInfo ReadNextKeyFromLoadedSession()
        {
            var keyData = _consoleKeyInfos.ElementAt(_currentIndex++);
            CurrentTime.SetToArtificialTime(keyData.Time);
            return keyData.GetKeyInfo();
        }

        private bool IsSessionEmpty()
        {
            return _consoleKeyInfos.Count == 0;
        }

        private void LoadNextSession()
        {
            _consoleKeyInfos = new PersistedList<KeyboardInputData>($"session-{_loadedSession++}", true);
            _currentIndex = 0;
        }

        private bool ShouldLoadNextSession()
        {
            return _consoleKeyInfos == null || _consoleKeyInfos.Count <= _currentIndex;
        }

        private ConsoleKeyInfo ReadAndRecordKey()
        {
            var key = Console.ReadKey(true);
            var keyData = new KeyboardInputData(key, DateTime.Now);
            _consoleKeyInfos.Add(keyData);
            return key;
        }
    }
}