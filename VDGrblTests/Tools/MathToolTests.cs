using VDGrbl.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media;
using System.Windows;

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

        [TestMethod()]
        public void MaxPointCollectionTest()
        {
            PointCollection points = new PointCollection
            {
                new Point(10, 23.1),
                new Point(7, 56.5)
            };
            var p = new Point(10, 56.5);
            Assert.AreEqual(p, MathTool.MaxPointCollection(points));
        }

        [TestMethod()]
        public void MinPointCollectionTest()
        {
            PointCollection points = new PointCollection
            {
                new Point(10, 23.1),
                new Point(7, 56.5)
            };
            var p = new Point(7, 23.1);
            Assert.AreEqual(p, MathTool.MinPointCollection(points));
        }
    }
}