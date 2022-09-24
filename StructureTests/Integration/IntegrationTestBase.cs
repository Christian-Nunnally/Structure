using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace StructureTests.Integration
{
    public class IntegrationTestBase
    {
        protected StructureTester Tester { get; set; }

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Tester = new StructureTester();
        }

        [TestCleanup]
        public void Cleanup()
        {
            Console.WriteLine($"Structure test finished: {TestContext.TestName}");
            Tester.PrintScreens();
        }
    }
}
