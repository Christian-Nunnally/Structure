using System;
using System.Threading;

namespace Structure
{
    public class Beeper : Module
    {
        private UserAction _action;
        private Thread _beepThread;

        public void Beep()
        {
            if (IO.SupressConsoleCalls) return;
            _beepThread = new Thread(() =>
            {
                var thisThread = _beepThread;
                Console.Beep();
                Thread.Sleep(1000 * 60 * 5);
                if (thisThread == _beepThread)
                {
                    Console.Beep();
                    Thread.Sleep(33);
                    Console.Beep();
                }
            });
            _beepThread.Start();
        }

        protected override void OnEnable()
        {
            _action = Hotkey.Add(ConsoleKey.B, new UserAction("Edit code", () => IO.Run(Beep)));
        }

        protected override void OnDisable()
        {
            Hotkey.Remove(ConsoleKey.B, _action);
        }
    }
}