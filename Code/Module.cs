using System;
using System.Collections.Generic;

namespace Structure
{
    internal abstract class Module : IModule
    {
        public abstract int RequiredLevel { get; }

        public string Name => GetType().Name;

        public virtual void Disable()
        {
        }

        public virtual void Enable()
        {
        }

        public virtual IEnumerable<(char, string, Action)> GetOptions() => new (char, string, Action)[0];
    }
}