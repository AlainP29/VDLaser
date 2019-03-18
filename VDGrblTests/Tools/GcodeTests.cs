using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDGrbl.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDGrbl.Tools.Tests
{
    [TestClass()]
    public class GcodeTests
    {
        [TestMethod()]
        public void FormatGcodeTest()
        {
            Assert.AreEqual("g90g1x10y-15.5z-2f300", Gcode.FormatGcode(0, 1, 2, -3.1, -0.4, 300, 5));
        }

        [TestMethod()]
        public void TrimGcodeTest()
        {

        }
    }
}