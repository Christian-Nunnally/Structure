using System;
using System.Collections.Generic;

namespace Structure
{
    internal interface IModule
    {
        string Name { get; }

        int RequiredLevel { get; }

        void Enable();

        void Disable();

        IEnumerable<(char, string, Action)> GetOptions();
    }
}