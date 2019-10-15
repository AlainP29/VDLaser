using VDGrbl.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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