﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDGrbl.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDGrbl.Tools.Tests
{
    [TestClass()]
    public class GCodeToolTests
    {
        GCodeTool gt = new GCodeTool();
        readonly string line = "G91 X10 Y15 F200 S30 M5";
        readonly List<string> list = new List<string> { "G91", "G1 X3 Y4 F100" };

        [TestMethod()]
        public void FormatGcodeTest()
        {
            Assert.AreEqual("g90g1x10y-15.5z-2f300", gt.FormatGcode(0, 1, 2, -3.1, -0.4, 300, 5));
        }

        [TestMethod()]
        public void TrimGcodeTest()
        {

        }

        [TestMethod()]
        public void SecondToTimeTest()
        {

        }

        
        [TestMethod()]
        public void ParseGcodeTestG()
        {
            gt = new GCodeTool(line);
            Assert.AreEqual("91", gt.G);
        }

        [TestMethod()]
        public void ParseGcodeTestX()
        {
            gt = new GCodeTool(line);
            Assert.AreEqual("10", gt.X);
        }

        [TestMethod()]
        public void ParseGcodeTestY()
        {
            gt = new GCodeTool(line);
            Assert.AreEqual("15", gt.Y);
        }

        [TestMethod()]
        public void ParseGcodeTestF()
        {
            gt = new GCodeTool(line);
            Assert.AreEqual("200", gt.F);
        }

        [TestMethod()]
        public void ParseGCodeTestS()
        {
            gt = new GCodeTool(line);
            Assert.AreEqual("30", gt.S);
        }

        [TestMethod()]
        public void ProcessMCodeTest()
        {
            gt = new GCodeTool(line);
            Assert.AreEqual(1, (int)gt.MCode);
        }

        [TestMethod()]
        public void ProcessGCodeTest()
        {
            gt = new GCodeTool(line);
            Assert.AreEqual(1, (int)gt.DMode);
        }

        [TestMethod()]
        public void CalculateJobTimeTestMMode()
        {
            gt = new GCodeTool(list);
            gt.CalculateJobTime(100);
            Assert.AreEqual(1, (int)gt.MMode);
        }

        [TestMethod()]
        public void CalculateJobTimeTestDMode()
        {
            gt = new GCodeTool(list);
            gt.CalculateJobTime(100);
            Assert.AreEqual(1, (int)gt.MMode);
        }
        [TestMethod()]
        public void CalculateJobTimeTestResult()
        {
            gt = new GCodeTool(list);
            Assert.AreEqual(3, gt.CalculateJobTime(100));
        }

    }
}