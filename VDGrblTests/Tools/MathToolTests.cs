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
    public class MathToolTests
    {
        [TestMethod()]
        public void DistanceTest()
        {
            Assert.AreEqual(5, MathTool.Distance(0, 0, 3, 4, 1));
        }
    }
}