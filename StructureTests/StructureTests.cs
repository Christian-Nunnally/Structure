using Microsoft.VisualStudio.TestTools.UnitTesting;
using Structur.Modules;
using Structur.Program;

namespace StructureTests.Server
{
    [TestClass]
    public class StructureTests
    {
        public StructureTester Tester { get; set; }

        [TestInitialize]
        public void ClientTestsInitialize()
        {
            Tester = new StructureTester();
        }

        [TestMethod]
        public void Run_DisplaysTitleScreen()
        {
            Tester.Run();
            Tester.Contains(StructureProgram.TitleString);
        }

        [TestMethod]
        public void Run_DisplaysModuleManagerOption()
        {
            Tester.Run();
            Tester.Contains(ModuleManager.ModuleStartHoykeyPrompt);
        }
    }
}