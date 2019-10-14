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
    public class IntToTimeConverterTests
    {
        [TestMethod()]
        public void ConvertTest()
        {
            IntToTimeConverter converter = new IntToTimeConverter();
            var value = converter.Convert(4, typeof(int), null, CultureInfo.CurrentCulture);
            Assert.AreEqual(new TimeSpan(4), value);
        }
    }
}