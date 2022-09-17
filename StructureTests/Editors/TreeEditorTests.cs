using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StructureTests.Editors
{
    [TestClass]
    public class TreeEditorTests
    {
        public StructureTester Tester { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            Tester = new StructureTester();
        }
    }
}
