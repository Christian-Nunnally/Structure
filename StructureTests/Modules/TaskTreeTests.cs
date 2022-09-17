using Microsoft.VisualStudio.TestTools.UnitTesting;
using Structur.Modules;
using StructureTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureTests.Editors
{
    [TestClass]
    public class TaskTreeTests
    {
        public StructureTester Tester { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Tester = new StructureTester();
        }
        
        [TestMethod]
        public void TaskTreeDisabled_TaskTreeHotkeyPressed_TaskTreeDoesNotOpen()
        {
            Tester.Run(HotkeyConstants.StartTaskTreeModuleHotkey);

            Tester.NotContains(TaskTree.Title);
        }

        [TestMethod]
        public void TaskTreeDisabled_EnableTaskTree_OpenTaskTreeHotkeyPromptVisible()
        {
            Tester.Run(
                ModuleManager.OpenModuleManagerHotkey,
                ModuleManager.EnableModuleHotkey,
                ModuleManager.EnableModuleHotkey
                );

            Tester.NotContains(TaskTree.Title);
        }
    }
}
