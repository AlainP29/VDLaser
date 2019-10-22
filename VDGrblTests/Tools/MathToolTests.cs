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
        public void DistanceTest1()
        {
            Assert.AreEqual(5, MathTool.Distance(3, 4));
        }

        [TestMethod()]
        public void DistanceTest2()
        {
            Assert.AreEqual(5, MathTool.Distance(0, 0, 3, 4));
        }
        [TestMethod()]
        public void DistanceTest3()
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

        [TestMethod()]
        public void RayonGCodeTest()
        {
            Assert.AreEqual(2, MathTool.RayonGCode(2, 3, 4, 3));
        }

        [TestMethod()]
        public void AngleChordAbscissaTest()
        {
            //double alpha =System.Math.Round(System.Math.PI / 4,15);
            double alpha = 0.785398163397448;
            Assert.AreEqual(alpha, System.Math.Round(MathTool.AngleChordAbscissa(0, 2, 1, 1,1,0)),15);
        }
    }
}