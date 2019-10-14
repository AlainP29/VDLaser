using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDGrbl.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace VDGrbl.Tools.Tests
{
    [TestClass()]
    public class DoubleConverterTests
    {
        [TestMethod()]
        public void ConvertTest()
        {
            DoubleConverter converter = new DoubleConverter();
            var value = converter.Convert(10.25, typeof(object), null, CultureInfo.CurrentCulture);
            double d=10.25;
            Assert.AreEqual(d, value);
        }
    }
}