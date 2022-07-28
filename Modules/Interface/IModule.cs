﻿using Structure.IO;
using Structure.Program;

namespace Structure.Modules.Interface
{
    public interface IModule
    {
        string Name { get; }

        bool Enabled { get; }

        public void Enable(StructureIoC ioc, StructureIO io);

        void Disable();
    }
}