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
    public class InverseBooleanConverterTests
    {
        [TestMethod()]
        public void ConvertTest()
        {
            InverseBooleanConverter converter = new InverseBooleanConverter();
            bool b = false;
            var value = (bool)converter.Convert(b, typeof(bool), null, CultureInfo.CurrentCulture);
            Assert.IsTrue(value);
        }

        [TestMethod()]
        public void ConvertTest1()
        {
            InverseBooleanConverter converter = new InverseBooleanConverter();
            bool b = false;
            var value = (bool)converter.Convert(b, typeof(bool), "parameter", CultureInfo.CurrentCulture);
            Assert.IsTrue(value);
        }
    }
}