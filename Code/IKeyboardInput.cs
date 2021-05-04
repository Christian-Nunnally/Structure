using System;

namespace Structure.Code
{
    public interface IKeyboardInput
    {
        public ConsoleKeyInfo ReadKey();

        public bool IsKeyAvailable();
    }
}