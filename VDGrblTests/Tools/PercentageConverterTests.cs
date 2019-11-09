using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDLaser.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace VDLaser.Tools.Tests
{
    [TestClass()]
    public class PercentageConverterTests
    {
        [TestMethod()]
        public void ConvertTest()
        {
            PercentageConverter converter = new PercentageConverter();
            double b = 53;
            var value = converter.Convert(b, typeof(double), null, CultureInfo.CurrentCulture);
            double r = 0.53;
            Assert.AreEqual(r, value);
        }

        [TestMethod()]
        public void ConvertParameterTest()
        {
            PercentageConverter converter = new PercentageConverter();
            double b = 500;
            int p = 25;
            var value = converter.Convert(b, typeof(double), p, CultureInfo.CurrentCulture);
            double r = 0.20;
            Assert.AreEqual(r, value);
        }
    }
}