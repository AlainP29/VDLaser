using VDGrbl.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace VDGrbl.Model.Tests
{
    
    [TestClass()]
    public class GCodeModelTests
    {
        GCodeModel gm;
        readonly string line = "G91 X10 Y15 F200 S30 M5";
        readonly List<string> list = new List<string> { "G91", "G1 X3 Y4 F100"};

        [TestMethod()]
        public void ParseGcodeTestG()
        {
            gm = new GCodeModel(line);
            Assert.AreEqual("91", gm.G);
        }

        [TestMethod()]
        public void ParseGcodeTestX()
        {
            gm = new GCodeModel(line);
            Assert.AreEqual("10", gm.X);
        }

        [TestMethod()]
        public void ParseGcodeTestY()
        {
            gm = new GCodeModel(line);
            Assert.AreEqual("15", gm.Y);
        }

        [TestMethod()]
        public void ParseGcodeTestF()
        {
            gm = new GCodeModel(line);
            Assert.AreEqual("200", gm.F);
        }

        [TestMethod()]
        public void ParseGCodeTestS()
        {
            gm = new GCodeModel(line);
            Assert.AreEqual("30", gm.S);
        }

        [TestMethod()]
        public void ProcessMCodeTest()
        {
            gm = new GCodeModel(line);
            Assert.AreEqual(1, (int)gm.MCode);
        }

        [TestMethod()]
        public void ProcessGCodeTest()
        {
            gm = new GCodeModel(line);
            Assert.AreEqual(1, (int)gm.DMode);
        }

        [TestMethod()]
        public void CalculateJobTimeTestMMode()
        {
            gm = new GCodeModel(list);
            gm.CalculateJobTime(100);
            Assert.AreEqual(1, (int)gm.MMode);
        }

        [TestMethod()]
        public void CalculateJobTimeTestDMode()
        {
            gm = new GCodeModel(list);
            gm.CalculateJobTime(100);
            Assert.AreEqual(1, (int)gm.MMode);
        }

        [TestMethod()]
        public void CalculateJobTimeTestResult()
        {
            gm = new GCodeModel(list);
            Assert.AreEqual(3, gm.CalculateJobTime(100));
        }
    }
}